using ClangPowerTools.Helpers;
using ClangPowerTools.Items;
using ClangPowerTools.Commands;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClangPowerTools
{
  public class ItemsCollector
  {
    #region Members

    private List<string> mAcceptedFileExtensions;
    private Array selectedItems;

    #endregion

    #region Constructor

    public ItemsCollector(List<string> aExtensions = null)
    {
      if(aExtensions == null)
      {
        mAcceptedFileExtensions = new List<string>(ScriptConstants.kAcceptedFileExtensions);
      }

      mAcceptedFileExtensions = aExtensions;
      var dte2 = (DTE2)VsServiceProvider.GetService(typeof(DTE));
      selectedItems = dte2.ToolWindows.SolutionExplorer.SelectedItems as Array;
    }

    #endregion 

    #region Properties

    public List<IItem> Items { get; private set; } = new List<IItem>();
    public bool HaveItems => Items.Count != 0;

    #endregion

    #region Public Methods

    // TODO : Refactor this method. Generics can be a solution.
    public void CollectActiveProjectItem()
    {
      try
      {
        DTE dte = (DTE)VsServiceProvider.GetService(typeof(DTE));
        Document activeDocument = dte.ActiveDocument;

        if (activeDocument == null)
          return;

        IItem item = null;
        if (SolutionInfo.IsOpenFolderModeActive())
        {
          item = new CurrentDocument(activeDocument);
        }
        else
        {
          item = new CurrentProjectItem(activeDocument.ProjectItem);
        }

        Items.Add(item);
      }
      catch (Exception e)
      {
        throw new Exception(e.Message);
      }
    }

    /// <summary>
    /// Get the name of the active document
    /// </summary>
    public List<string> GetFilesToIgnore()
    {
      CollectSelectedProjectItems();
      List<string> documentsToIgnore = new List<string>();
      Items.ForEach(e => documentsToIgnore.Add(e.GetName()));

      return documentsToIgnore;
    }

    public List<string> GetProjectsToIgnore()
    {
      List<string> projectsToIgnore = new List<string>();
      DTE2 dte2 = VsServiceProvider.GetService(typeof(DTE)) as DTE2;
      Array selectedItems = dte2.ToolWindows.SolutionExplorer.SelectedItems as Array;

      foreach (UIHierarchyItem item in selectedItems)
      {
        if (item.Object is Project)
        {
          var project = item.Object as Project;
          projectsToIgnore.Add(project.Name);
        }
      }

      return projectsToIgnore;
    }

    /// <summary>
    /// Get selected files to encode
    /// </summary>
    public static List<string> GetDocumentsToEncode()
    {
        var itemsCollector = CompileCommand.Instance.ItemsCollector;
        itemsCollector.CollectCurrentProjectItems();
        HashSet<string> selectedFiles = new HashSet<string>();
        itemsCollector.Items.ForEach(i => selectedFiles.Add(i.GetPath()));
        return selectedFiles.ToList();
    }

    /// <summary>
    /// Collect all selected items in the Solution explorer for commands
    /// </summary>
    public void CollectSelectedItems()
    {
      if (selectedItems == null || selectedItems.Length == 0)
        return;

      foreach (UIHierarchyItem item in selectedItems)
      {
        if (item.Object is Solution)
        {
          var solution = item.Object as Solution;
          GetProjectsFromSolution(solution);
        }
        else if (item.Object is Project)
        {
          var project = item.Object as Project;
          AddProject(project);
        }
        else if (item.Object is ProjectItem)
        {
          GetProjectItem(item.Object as ProjectItem);
        }
      }
    }

    /// <summary>
    /// Collect all selected ProjectItems
    /// </summary>
    public void CollectSelectedProjectItems()
    {
      if (selectedItems == null || selectedItems.Length == 0)
        return;

      foreach (UIHierarchyItem item in selectedItems)
      {
        if (item.Object is Solution)
        {
          var solution = item.Object as Solution;
          GetProjectItem(solution);
        }
        else if (item.Object is Project)
        {
          var project = item.Object as Project;
          GetProjectItem(project);
        }
        else if (item.Object is ProjectItem)
        {
          GetProjectItem(item.Object as ProjectItem);
        }
      }
    }

    public void CollectCurrentProjectItems()
    {
      if (selectedItems == null || selectedItems.Length == 0)
        return;

      foreach (UIHierarchyItem item in selectedItems)
      {
        if (item.Object is Solution)
        {
          var solution = item.Object as Solution;
          GetProjectItem(solution);
        }
        else if (item.Object is Project)
        {
          var project = item.Object as Project;
          GetProjectItem(project);
        }
        else if (item.Object is ProjectItem)
        {
          Project project = (item.Object as ProjectItem).ContainingProject;
          GetProjectItem(project);
          return;
        }
      }
    }

    public void AddProjectItem(ProjectItem aItem)
    {
      if (aItem == null)
        return;

      var fileExtension = Path.GetExtension(aItem.Name).ToLower();
      if (null != mAcceptedFileExtensions && false == mAcceptedFileExtensions.Contains(fileExtension))
        return;

      Items.Add(new CurrentProjectItem(aItem));
    }

    #endregion


    #region Private Methods


    private void GetProjectsFromSolution(Solution aSolution)
    {
      Items = AutomationUtil.GetAllProjects(aSolution);
    }


    private void AddProject(Project aProject) => Items.Add(new CurrentProject(aProject));


    private void GetProjectItem(ProjectItem aProjectItem)
    {
      // Items that contains projects
      if (aProjectItem.ProjectItems == null)
      {
        if (aProjectItem.SubProject != null)
          AddProject(aProjectItem.SubProject);
        return;
      }
      // Folders or filters
      else if (aProjectItem.ProjectItems.Count != 0)
      {
        foreach (ProjectItem projItem in aProjectItem.ProjectItems)
          GetProjectItem(projItem);
      }
      // Files
      else
      {
        AddProjectItem(aProjectItem);
      }
    }


    private void GetProjectItem(Project aProject)
    {
      foreach (var item in aProject.ProjectItems)
      {
        var projectItem = item as ProjectItem;
        if (projectItem == null)
          continue;

        GetProjectItem(projectItem);
      }
    }


    private void GetProjectItem(Solution aSolution)
    {
      foreach (var item in AutomationUtil.GetAllProjects(aSolution))
      {
        var project = (item as CurrentProject).GetObject() as Project;
        if (project == null)
          continue;

        GetProjectItem(project);
      }
    }

    #endregion
  }
}
