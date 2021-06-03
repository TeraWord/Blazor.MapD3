using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TeraWord.Blazor.MapD3
{
    public partial class MapD3 : IDisposable
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }

        [Parameter] public string ID { get; set; }

        [Parameter] public string Service { get; set; }

        [Parameter] public string Style { get; set; }

        [Parameter] public int Width { get; set; }

        [Parameter] public int Height { get; set; }

        [Parameter] public Data Data { get; set; }

        [Parameter] public bool ZoomEnabled { get; set; }

        [Parameter] public bool ShowControls { get; set; }

        [Parameter] public EventCallback<Node> OnNodeClick { get; set; }

        [JSInvokable] public async Task OnInternalNodeClick(Node node)
        {
            await OnNodeClick.InvokeAsync(node);
        }

        private DotNetObjectReference<MapD3> MapD3Instance;

        private IJSObjectReference MapD3Module;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            MapD3Instance = DotNetObjectReference.Create(this);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                MapD3Module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/teraword.blazor.mapd3/mapd3.js");

                await MapD3Module.InvokeVoidAsync("MapD3Init", ID, Width, Height, MapD3Instance, Service);

                await MapD3Module.InvokeVoidAsync("MapD3InitPanZoom", ID, ZoomEnabled, ShowControls);

                //await MapD3Module.InvokeVoidAsync("MapD3Load", Data);
            }
            else
            {
                //await MapD3Module.InvokeVoidAsync("MapD3Update", Data);
            }

            await MapD3Module.InvokeVoidAsync("MapD3Load", Data);
        }

        private async void Clicked()
        {
            await JSRuntime.InvokeVoidAsync("MapD3Refresh");
        }

        private string InternalStyle
        {
            get
            {
                var style = new StringBuilder();
                style.Append("position:relative;");
                style.Append(Style);
                return style.ToString();
            }
        }

        public void Dispose()
        {
            MapD3Instance?.Dispose();

            MapD3Module?.DisposeAsync();
        }
    }
}
