using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TeraWord.Blazor.MapD3
{
    public partial class D3Map : ComponentBase, IDisposable
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }

        [Parameter] public string ID { get; set; }

        [Parameter] public string Service { get; set; }

        [Parameter] public string Style { get; set; }

        [Parameter] public string Width { get; set; } = "100%";

        [Parameter] public string Height { get; set; } = "500px";

        public D3Data Data { get; set; }

        [Parameter] public bool ZoomEnabled { get; set; }

        [Parameter] public bool ShowControls { get; set; }

        [Parameter] public int LinkDistance { get; set; } = 60;

        [Parameter] public int LinkLengths { get; set; } = 20;

        [Parameter] public EventCallback<D3Node> OnNodeClick { get; set; }

        private DotNetObjectReference<D3Map> Instance { get; set; }

        private IJSObjectReference Module { get; set; }

        private string LastData { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                if (Module is null)
                {
                    await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/teraword.blazor.mapd3/cola.d3v7.js");
                    Module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/teraword.blazor.mapd3/mapd3.js");
                }

                Instance ??= DotNetObjectReference.Create(this);

                if (Module is not null) await Module.InvokeVoidAsync("MapD3Init", ID, Width, Height, LinkDistance, LinkLengths, Instance, Service);

                await Update();
                await ZoomToCenter(1.5);
            }
        }

        [JSInvokable]
        public async Task OnInternalNodeClick(D3Node node)
        {
            await OnNodeClick.InvokeAsync(node);
        }

        private async Task Clicked()
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3Refresh");
        }

        private async Task Clear()
        {
            LastData = null;

            if (Module is not null) await Module.InvokeVoidAsync("MapD3Clear");
        }

        private string InternalStyle
        {
            get
            {
                var style = new StringBuilder();
                style.Append("position:relative;");
                style.Append("width:100%;");
                style.Append("height:100%;");
                style.Append(Style);
                return style.ToString();
            }
        }

        public async Task ZoomToFit()
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3ZoomToFit");
        }

        public async Task ZoomTo(double x, double y, double s)
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3ZoomTo", x, y, s);
        }

        public async Task ZoomToCenter(double s)
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3ZoomToCenter", s);
        }

        public async Task SetLinkDistance(int distance)
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3SetLinkDistance", distance);
        }

        public async Task SetLinkLengths(int lengths)
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3SetSymmetricDiffLinkLengths", lengths);
        }

        public async Task Update()
        {
            if (JsonConvert.SerializeObject(Data) != LastData)
            {
                LastData = JsonConvert.SerializeObject(Data);

                if (Module is not null) await Module.InvokeVoidAsync("MapD3Update", Data?.Compile());
            }
        } 

        public void Dispose()
        {
            if (Instance is not null) Instance.Dispose();
            Instance = null;

            if (Module is not null) Module.DisposeAsync();
            Module = null;
        }
    }
}
