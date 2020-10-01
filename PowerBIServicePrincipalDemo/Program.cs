using Microsoft.PowerBI.Api.Models;
using PowerBIServicePrincipalDemo.Models;

namespace PowerBIServicePrincipalDemo {

  class Program {
  
    static void Main(string[] args) {

      PowerBiManagerAppOnly.GetAppWorkspaces();
      // PowerBiManagerAppOnly.CreateAppWorkspace("Test Workspace 1");
      // CreateAndPopulateWorkspaceAsServicePrincipal();
    }

    static void CreateAndPopulateWorkspaceAsServicePrincipal() {
      PowerBiManagerAppOnly.PublishContent("Test Workspace 2");
    }

    static void TakeOverDatasetAndRefreshTest() {
      Group workspace = PowerBiManagerAppOnly.GetAppWorkspace("Test Workspace 2");
      Dataset dataset = PowerBiManagerAppOnly.GetDataset(workspace.Id, "Wingtip Sales");
      PowerBiManagerAppOnly.TakeOverDatasetAndRefresh(workspace.Id, dataset.Id, "CptStudent", "pass@word1");

    }

  }
}
