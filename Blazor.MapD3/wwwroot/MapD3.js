var _MapD3;
var _MapD3Instance;

export function MapD3Init(div, width, height, instance, service) {
    _MapD3Instance = instance;
    _MapD3 = new MapD3(width, height, div, service, MapD3OnInternalNodeClick);
    //if(service != null) _MapD3.Load(_MapD3); 
}

function MapD3Edit() {
    
}

function MapD3Refresh() {
    _MapD3.Update(_MapD3);
}

export function MapD3Load(data) {
    _MapD3.LoadGraph(_MapD3, data);
}

export function MapD3Update(data) {
    _MapD3.UpdateGraph(_MapD3, data);
}

function MapD3OnInternalNodeClick(e) {
    var node = {
        Code: e.code,
        Label: e.label,
        Parents: e.parents,
        Group: e.group,
        Tooltip: e.tooltip,
        Color: e.color,
        Header: e.header,
        Footer: e.footer,
        Data: e.data
    };

    _MapD3Instance.invokeMethodAsync('OnInternalNodeClick', node);
}

function MapD3ShowTooltip(evt, node) {
    let tooltip = document.getElementById("d3tooltip");
    tooltip.innerHTML = node.tooltip;
    tooltip.style.display = "block";
    tooltip.style.left = evt.pageX - 20 + 'px';
    tooltip.style.top = evt.pageY - 30 + 'px';
}

function MapD3HideTooltip() {
    var tooltip = document.getElementById("d3tooltip");
    tooltip.style.display = "none";
}

/* -- svg-panzoom integration -------------------------------------------------------------------------- */

export function MapD3InitPanZoom(div, zoomEnabled, showControls) {
    svgPanZoom("#svg-" + div, {
        zoomEnabled: zoomEnabled,
        controlIconsEnabled: showControls,
        fit: true,
        center: true
    });
}

    //document.getElementById('enable').addEventListener('click', function () {
    //    window.zoomTiger.enableControlIcons();
    //})

    //document.getElementById('disable').addEventListener('click', function () {
    //    window.zoomTiger.disableControlIcons();
    //})


/* -----------------------------------------------------------------------------------------------------------------------------  */

function MapD3(width, height, div, action, onNodeClick) {
    this.Width = width;
    this.Height = height;
    this.IsMouseDown = false;
    this.Action = action;
    this.OnNodeClick = onNodeClick;

    var map = this;

    this.Cola = cola.d3adaptor(d3)
        .avoidOverlaps(true)
        .handleDisconnected(true)
        .size([width, height]);

    this.Svg = d3.select('#' + div)
        .append("svg")
        .attr("width", "100%")
        .attr("height", height)
        .attr("id", "svg-" + div);

    this.Svg.append('rect')
        .attr('width', "100%")
        .attr('height', "100%")
        .style('fill', 'white');

    this.Area = map.Svg.append("g");

    this.Svg.call(d3.zoom()
        .extent([[0, 0], [width, height]])
        .scaleExtent([1, 8])
        .on("zoom", function () {
            map.Area.attr("transform", d3.event.transform);
        }));
}

MapD3.prototype.Load = function (map) {
    //d3.json(map.Action, function (graph) {
    $.getJSON(map.Action, function (graph) {
        LoadGraph(map, graph);
    })
    .done(function () {
        console.log("second success");
    })
    .fail(function () {
        console.log("error");
    })
    .always(function () {
        console.log("complete");
    });
};

MapD3.prototype.LoadGraph = function (map, graph) {
    map.Area.selectAll("*").remove();

    if (graph == null) return;

    map.Cola.nodes(graph.nodes)
        .links(graph.links)
        //.groups(graph.groups)
        .jaccardLinkLengths(60, 0.8)
        .start(30, 30, 30);
    
    map.Links = map.Area.selectAll(".link")
        .data(graph.links)
        .enter().append("line")
        .attr("class", "d3link");

    map.Nodes = map.Area.selectAll(".node")
        .data(graph.nodes)
        .enter().append("rect")
        .attr("class", "d3node")
        .attr("width", function (d) {
            d.width = map.ComputedTextLength(map, d.label) + 10;
            return d.width;
        })
        .attr("height", function (d) {
            d.height = 12;
            return d.height;
        })
        .attr("rx", 5).attr("ry", 5)
        .style("fill", function (d) {
            return d.color;
        })
        .on("mouseover", function (d) {
            map.OnMouseOver(d3.event, map, d);
        })
        .on("mouseout", function (d, i) {            
            map.OnMouseOut(map, d);
        })
        .on("click", function (d, i) {
            map.OnMouseClick(map, d);
        })
        .call(map.Cola.drag);

    map.Labels = map.Area.selectAll(".label")
        .data(graph.nodes).enter()
        .append("text").attr("class", "d3label") 
        .text(function (d) { return d.label; });

    map.LabelsHeader = map.Area.selectAll(".label")
        .data(graph.nodes).enter()
        .filter(function (d) { return d.header !== null; })
        .append("text").attr("class", "d3labelHeader")
        .text(function (d) { return d.header; });

    map.LabelsFooter = map.Area.selectAll(".label")
        .data(graph.nodes).enter()
        .filter(function (d) { return d.footer !== null; })
        .append("text").attr("class", "d3labelFooter")
        .text(function (d) { return d.footer; });

    map.Cola.on("tick", function () {
        map.Links
            .attr("x1", function (d) { return d.source.x; })
            .attr("y1", function (d) { return d.source.y; })
            .attr("x2", function (d) { return d.target.x; })
            .attr("y2", function (d) { return d.target.y; });

        map.Nodes
            .attr("x", function (d) { return d.x - d.width / 2; })
            .attr("y", function (d) { return d.y - d.height / 2; });

        map.Nodes
            .attr("cx", function (d) { return d.x; })
            .attr("cy", function (d) { return d.y; });

        map.Labels
            .attr("x", function (d) { return d.x; })
            .attr("y", function (d) { return d.y + 3; });

        map.LabelsHeader
            .attr("x", function (d) { return d.x; })
            .attr("y", function (d) { return d.y - 8; });

        map.LabelsFooter
            .attr("x", function (d) { return d.x; })
            .attr("y", function (d) { return d.y + 12; });
    });
};

MapD3.prototype.Update = function (map) {
    //d3.json(map.Action, function (error, graph) {
    $.getJSON(map.Action, function (graph) {
        UpdateGraph(map, graph);
    })
    .done(function () {
        console.log("second success");
    })
    .fail(function () {
        console.log("error");
    })
    .always(function () {
        console.log("complete");
    });
};

MapD3.prototype.UpdateGraph = function (map, graph) {
    var nodes = map.Nodes.attr("class", "d3node");
    var links = map.Links.attr("class", "d3link");
    var labels = map.Labels.attr("class", "d3label");
    var labelsHeader = map.LabelsHeader.attr("class", "d3labelHeader");
    var labelsFooter = map.LabelsFooter.attr("class", "d3labelFooter");

    $.each(graph.nodes, function (i, node) {
        var code = node.code;

        nodes.filter(function (d) { return d.code === code; })
            .attr("width", function (d) {
                d.width = map.ComputedTextLength(map, node.label) + 10;
                return d.width;
            })
            .style("fill", function (d) {
                d.color = node.color;
                return d.color;
            })
            .attr("x", function (d) { return d.x - d.width / 2; })
            .attr("y", function (d) { return d.y - d.height / 2; });

        labels.filter(function (d) { return d.code === code; })
            .text(function (d) {
                d.label = node.label;
                return d.label;
            });

        labelsHeader.filter(function (d) { return d.code === code; })
            .text(function (d) {
                if (node.header !== null) {
                    return node.header;
                }
                else {
                    return "";
                }
            });

        labelsFooter.filter(function (d) { return d.code === code; })
            .text(function (d) {
                if (node.footer !== null) {
                    return node.footer;
                }
                else {
                    return "";
                }
            });
    });    
};

MapD3.prototype.OnMouseOver = function (evt, map, node) {
    if (node === null) return;
    MapD3ShowTooltip(evt, node);
};

MapD3.prototype.OnMouseOut = function (map, node) {
    MapD3HideTooltip();
    if (node === null) return;    
};

MapD3.prototype.OnMouseClick = function (map, node) {
    if (node === null) return;
    map.OnNodeClick(node);
};

MapD3.prototype.ComputedTextLength = function (map, text) {
    var id = "ERTWERTWERTWERTWERTWE";

    var m = map.Area
        .append("text")
        .attr("id", id)
        .attr("class", "d3label")
        .style("fill", "transparent");

    var length = m.text(text).node().getComputedTextLength();

    d3.select("#ERTWERTWERTWERTWERTWE").remove();

    return length;
};