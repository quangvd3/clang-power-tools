﻿using ClangPowerTools.Events;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Controllers
{
  public class LicenseController
  {
    #region Members

    public static event EventHandler<LicenseEventArgs> OnLicenseStatusChanced;
    private readonly string fileName = "ctpjwt";

    #endregion

    #region Public Methods

    public async Task<bool> CheckLicenseAsync(TokenModel tokenModel = null)
    {
      var networkAvailable = await NetworkUtility.CheckInternetConnectionAsync();
      tokenModel ??= new TokenModel();

      bool licenceStatus = networkAvailable ? await CheckOnlineLicenseAsync(tokenModel) : CheckLocalLicense();

      OnLicenseStatusChanced.Invoke(this, new LicenseEventArgs(licenceStatus));
      return licenceStatus;
    }

    public bool CheckLocalLicense()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath(fileName);
      return File.Exists(filePath);
    }

    public async Task<bool> CheckOnlineLicenseAsync(TokenModel tokenModel = null)
    {
      if (TokenExists(tokenModel, out TokenModel newToken) == false)
      {
        return false;
      }

      try
      {
        HttpResponseMessage userTokenHttpResult = await CheckUserTokenHttpResultAsync(newToken);
        return userTokenHttpResult.IsSuccessStatusCode ? await CheckAllUserLicensesAsync(userTokenHttpResult) : false;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public async Task<HttpResponseMessage> CheckUserTokenHttpResultAsync(TokenModel token)
    {
      if (ApiUtility.ApiClient == null)
      {
        ApiUtility.InitializeApiClient();
      }

      ApiUtility.ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.jwt);
      return await ApiUtility.ApiClient.GetAsync(WebApiUrl.licenseUrl);
    }

    public bool TokenExists(TokenModel token, out TokenModel newToken)
    {
      newToken = CheckToken(token);
      return newToken != null && string.IsNullOrWhiteSpace(newToken.jwt) == false;
    }

    #endregion

    #region Private Methods

    private async Task<bool> CheckAllUserLicensesAsync(HttpResponseMessage result)
    {
      List<LicenseModel> licenses = await result.Content.ReadAsAsync<List<LicenseModel>>();
      return licenses.Any(license => CheckExpirationDate(license)) == false;
    }

    private bool CheckExpirationDate(LicenseModel license)
    {
      DateTime.TryParse(license.expires, out DateTime expirationDate);
      return DateTime.Compare(DateTime.Now, expirationDate) < 0;
    }

    private TokenModel CheckToken(TokenModel tokenModel)
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath("ctpjwt");

      if (File.Exists(filePath) == false)
        return tokenModel;

      using var streamReader = new StreamReader(filePath);
      tokenModel.jwt = streamReader.ReadLine();
      
      return tokenModel;
    }

    #endregion
  }
}
