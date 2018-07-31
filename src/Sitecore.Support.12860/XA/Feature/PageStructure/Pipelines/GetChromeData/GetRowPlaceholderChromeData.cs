using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Pipelines.GetChromeData;
using Sitecore.StringExtensions;
using Sitecore.Web;
using Sitecore.XA.Feature.PageStructure;
using Sitecore.XA.Feature.PageStructure.Models;
using Sitecore.XA.Feature.PageStructure.Pipelines.GetChromeData;
using Sitecore.XA.Foundation.Abstractions;
using Sitecore.XA.Foundation.SitecoreExtensions.Repositories;

namespace Sitecore.Support.XA.Feature.PageStructure.Pipelines.GetChromeData
{
  public class GetRowPlaceholderChromeData : Sitecore.XA.Feature.PageStructure.Pipelines.GetChromeData.GetRowPlaceholderChromeData
  {
    protected virtual bool IsButtonAdded(WebEditButton button, GetChromeDataArgs args)
    {
      return args.ChromeData.Commands.Any(b => b.Click.Contains(button.Click) && b.Header.Equals(button.Header, StringComparison.OrdinalIgnoreCase) && b.Tooltip.Equals(button.Tooltip, StringComparison.OrdinalIgnoreCase));
    }

    public override void Process(GetChromeDataArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      Assert.IsNotNull(args.ChromeData, "Chrome Data");
      if ("placeholder".Equals(args.ChromeType, StringComparison.OrdinalIgnoreCase))
      {
        string argument = args.CustomData["placeHolderKey"] as string;
        object[] parameters = new object[] { "placeHolderKey" };
        Assert.ArgumentNotNull(argument, "CustomData[\"{0}\"]".FormatWith(parameters));
        if (this.IsSplitterPlaceholder(args))
        {
          bool flag = false;
          DeviceDefinition definition = LayoutDefinition.Parse(WebUtil.GetSessionString(this.PageDesigner.PageDesignerHandle)).Devices.Cast<DeviceDefinition>().FirstOrDefault<DeviceDefinition>(deviceDefinition => new ID(deviceDefinition.ID).Equals(base.Context.Device.ID));
          string renderingId = null;
          if (definition != null)
          {
            IEnumerable<RenderingDefinition> enumerable = definition.Renderings.Cast<RenderingDefinition>();
            SpliterCheckModel model2 = this.IsSplitterPlaceholder(argument, from r in enumerable select new RenderingDescription(r));
            if (model2.IsSpliter)
            {
              flag = true;
              renderingId = model2.UniqueColumnSpliterId;
            }
          }
          RenderingReference[] renderings = args.CommandContext.Items[0].Visualization.GetRenderings(base.Context.Device, true);
          SpliterCheckModel model = this.IsSplitterPlaceholder(argument, from r in renderings select new RenderingDescription(r));
          if (model.IsSpliter)
          {
            flag = true;
            renderingId = model.UniqueColumnSpliterId;
          }
          if (flag)
          {
            this.FillCommandContext(args, renderingId);
            var webButton = ConvertToWebEditButton(ServiceLocator.ServiceProvider.GetService<IDatabaseRepository>().GetDatabase("core").GetItem(Items.EditColumnGridParametersEditFrameButton));
            if (!IsButtonAdded(webButton, args))
            {
              this.AddButtonToChromeData(this.ConvertToWebEditButton(ServiceProviderServiceExtensions.GetService<IDatabaseRepository>(ServiceLocator.ServiceProvider).GetDatabase("core").GetItem(Items.EditColumnGridParametersEditFrameButton)), args);
            }
            this.FillCustomData(args, renderingId);
          }
        }
      }
    }
  }
}