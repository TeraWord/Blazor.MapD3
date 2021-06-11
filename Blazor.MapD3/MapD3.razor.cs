using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TeraWord.Blazor.MapD3
{
    public partial class MapD3 : ComponentBase, IDisposable
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }

        [Parameter] public string ID { get; set; }

        [Parameter] public string Service { get; set; }

        [Parameter] public string Style { get; set; }

        [Parameter] public string Width { get; set; } = "100%";

        [Parameter] public string Height { get; set; } = "500px";

        [Parameter] public Data Data { get => _Data; set { _Data = value; _ = Update(); } }
        private Data _Data;

        [Parameter] public bool ZoomEnabled { get; set; }

        [Parameter] public bool ShowControls { get; set; }

        [Parameter] public EventCallback<Node> OnNodeClick { get; set; }

        [JSInvokable] public async Task OnInternalNodeClick(Node node)
        {
            await OnNodeClick.InvokeAsync(node);
        }

        private DotNetObjectReference<MapD3> Instance;

        private IJSObjectReference Module;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                if (Module is null)
                {
                    await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/teraword.blazor.mapd3/cola.d3v6.js");
                    Module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/teraword.blazor.mapd3/mapd3.js");
                }
                if (Instance is null) Instance = DotNetObjectReference.Create(this);
                if (Module is not null) await Module.InvokeVoidAsync("MapD3Init", ID, Width, Height, Instance, Service);
        
                await Update();
                await ZoomToCenter(1.5);
            }   
        }

        private async void Clicked()
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3Refresh");
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

        public async Task Update()
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3Update", Data?.Compile());
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
