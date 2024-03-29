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

        [Parameter] public int LinkDistance { get => _LinkDistance; set { if (_LinkDistance != value) { _LinkDistance = value; _ = SetLinkDistance(value); } } }
        private int _LinkDistance = 60;

        [Parameter] public int LinkLengths { get => _LinkLengths; set { if (_LinkLengths != value) { _LinkLengths = value; _ = SetLinkLengths(value); } } }
        private int _LinkLengths = 20;

        [Parameter] public EventCallback<Node> OnNodeClick { get; set; }

        private DotNetObjectReference<MapD3> Instance { get; set; }

        private IJSObjectReference Module { get; set; }

        private Guid LastDataID { get; set; }

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
                if (Instance is null) Instance = DotNetObjectReference.Create(this);
                if (Module is not null) await Module.InvokeVoidAsync("MapD3Init", ID, Width, Height, LinkDistance, LinkLengths, Instance, Service);

                await Update();
                await ZoomToCenter(1.5);
            }
        }

        [JSInvokable]
        public async Task OnInternalNodeClick(Node node)
        {
            await OnNodeClick.InvokeAsync(node);
        }

        private async void Clicked()
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3Refresh");
        }

        private async Task Clear()
        {
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

        private async Task SetLinkDistance(int distance)
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3SetLinkDistance", distance);
        }

        private async Task SetLinkLengths(int lengths)
        {
            if (Module is not null) await Module.InvokeVoidAsync("MapD3SetSymmetricDiffLinkLengths", lengths);
        }

        public async Task Update()
        {
            if (Data is not null && !LastDataID.Equals(Data))
            {
                LastDataID = Data.ID;
                await Clear();
            }

            if (Module is not null) await Module.InvokeVoidAsync("MapD3Update", Data?.Compile());
        }

        public async Task Update(Data data)
        {
            _Data = data;
            await Update();
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
