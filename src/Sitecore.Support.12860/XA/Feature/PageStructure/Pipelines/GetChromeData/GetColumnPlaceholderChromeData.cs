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
  public class GetColumnPlaceholderChromeData: Sitecore.XA.Feature.PageStructure.Pipelines.GetChromeData.GetColumnPlaceholderChromeData
  {
    protected virtual bool IsButtonAdded(WebEditButton button, GetChromeDataArgs args)
    {
      return args.ChromeData.Commands.Any(b => b.Click.Contains(button.Click) && b.Header.Equals(button.Header, StringComparison.OrdinalIgnoreCase) && b.Tooltip.Equals(button.Tooltip, StringComparison.OrdinalIgnoreCase));
    }

    public override void Process(GetChromeDataArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      Assert.IsNotNull(args.ChromeData, "Chrome Data");
      if (!"placeholder".Equals(args.ChromeType, StringComparison.OrdinalIgnoreCase))
      {
        return;
      }
      string placeholderKey = args.CustomData["placeHolderKey"] as string;
      Assert.ArgumentNotNull(placeholderKey, "CustomData[\"{0}\"]".FormatWith("placeHolderKey"));

      if (IsSplitterPlaceholder(args))
      {
        bool appendGridPropertiesButton = false;
        // Session renderings
        var devices = LayoutDefinition.Parse(WebUtil.GetSessionString(PageDesigner.PageDesignerHandle)).Devices.Cast<DeviceDefinition>();
        var currentDevice = devices.FirstOrDefault(definition => (new ID(definition.ID)).Equals(Context.Device.ID));
        string uniqueRenderingId = null;
        if (currentDevice != null)
        {
          var renderingsFromSession = currentDevice.Renderings.Cast<RenderingDefinition>();
          var sessionColumnSpliterCheckData = IsSplitterPlaceholder(placeholderKey, renderingsFromSession.Select(r => new RenderingDescription(r)));
          if (sessionColumnSpliterCheckData.IsSpliter)
          {
            appendGridPropertiesButton = true;
            uniqueRenderingId = sessionColumnSpliterCheckData.UniqueColumnSpliterId;
          }
        }

        // Field renderings
        var currentItem = args.CommandContext.Items[0];
        var renderingsFromField = currentItem.Visualization.GetRenderings(Context.Device, true);
        var fieldColumnSpliterCheckData = IsSplitterPlaceholder(placeholderKey, renderingsFromField.Select(r => new RenderingDescription(r)));
        if (fieldColumnSpliterCheckData.IsSpliter)
        {
          appendGridPropertiesButton = true;
          uniqueRenderingId = fieldColumnSpliterCheckData.UniqueColumnSpliterId;
        }

        if (appendGridPropertiesButton)
        {
          FillCommandContext(args, uniqueRenderingId);
          var webButton = ConvertToWebEditButton(ServiceLocator.ServiceProvider.GetService<IDatabaseRepository>().GetDatabase("core").GetItem(Items.EditColumnGridParametersEditFrameButton));
          if (!IsButtonAdded(webButton, args))
          {
            AddButtonToChromeData(webButton, args);
          }

          FillCustomData(args, uniqueRenderingId);
        }
      }
    }
  }
}