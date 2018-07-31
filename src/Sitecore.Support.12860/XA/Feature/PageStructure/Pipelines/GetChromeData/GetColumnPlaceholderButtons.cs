using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Pipelines.GetChromeData;
using Sitecore.StringExtensions;
using Sitecore.Web;
using Sitecore.XA.Feature.PageStructure;
using Sitecore.XA.Feature.PageStructure.Pipelines.GetChromeData;
using Sitecore.XA.Foundation.SitecoreExtensions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.Support.XA.Feature.PageStructure.Pipelines.GetChromeData
{
  public class GetColumnPlaceholderButtons: Sitecore.XA.Feature.PageStructure.Pipelines.GetChromeData.GetColumnPlaceholderButtons
  {
    protected virtual bool IsButtonAdded(WebEditButton button, GetChromeDataArgs args)
    {
      return args.ChromeData.Commands.Any(b => b.Click.Contains(button.Click) && b.Header.Equals(button.Header, StringComparison.OrdinalIgnoreCase) && b.Tooltip.Equals(button.Tooltip, StringComparison.OrdinalIgnoreCase));
    }
    protected override void AddButtonById(ID buttonItemId, GetChromeDataArgs args)
    {
      var webEditButton = ConvertToWebEditButton(ServiceLocator.ServiceProvider.GetService<IDatabaseRepository>().GetDatabase("core").GetItem(buttonItemId));
      if (!IsButtonAdded(webEditButton, args))
      {
        AddButtonToChromeData(webEditButton, args);
      }
    }
    
  }
}