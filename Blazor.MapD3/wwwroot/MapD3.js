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
        ID: e.id,
        Code: e.code,
        Name: e.name,
        Parent: e.parent,
        Group: e.group,
        Hint: e.hint,
        Color: e.color,
        Header: e.header,
        Footer: e.footer,
        Data: e.data
    };

    MapD3Instance.invokeMethodAsync('MapD3OnInternalNodeClick', node);
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

    this.Outer = d3.select('#' + div)
        .append("svg")
        .attr("width", "100%")
        .attr("height", height)
        .attr("id", "svg-" + div)
        .attr("pointer-events", "all");

    this.Zoom = d3.zoom()
        .scaleExtent([0.1, 32])
        .translateExtent([[-1000, -1000], [width + 1000, height + 1000]]);

    this.Outer.append('rect')
        .attr('width', "100%")
        .attr('height', "100%")
        .style('fill', 'white')
        .call(map.Zoom.on("zoom", function (a, b, c, d) {
            map.Redraw(map, b);
        }));

    this.Vis = map.Outer.append('g');
    this.Svg = map.Vis.append("g");
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
    map.Svg.selectAll("*").remove();

    if (graph == null) return;

    map.Cola.nodes(graph.nodes)
        .links(graph.links)
        //.groups(graph.groups)
        .jaccardLinkLengths(60, 0.8)
        .start(30, 30, 30);
    
    map.Links = map.Svg.selectAll(".link")
        .data(graph.links)
        .enter().append("line")
        .attr("class", "d3link");

    map.Nodes = map.Svg.selectAll(".node")
        .data(graph.nodes)
        .enter().append("rect")
        .attr("class", "d3node")
        .attr("width", function (d) {
            d.width = map.ComputedTextLength(map, d.name) + 10;
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
        .on("mouseover", function (d, i) {
            map.OnMouseOver(map, d);
        })
        .on("mouseout", function (d, i) {
            map.OnMouseOut(map, d);
        })
        .on("click", function (d, i) {
            map.OnMouseClick(map, d);
        })
        .call(map.Cola.drag);

    map.Labels = map.Svg.selectAll(".label")
        .data(graph.nodes).enter()
        .append("text").attr("class", "d3label") 
        .text(function (d) { return d.name; });

    map.LabelsHeader = map.Svg.selectAll(".label")
        .data(graph.nodes).enter()
        .filter(function (d) { return d.header !== null; })
        .append("text").attr("class", "d3labelHeader")
        .text(function (d) { return d.header; });

    map.LabelsFooter = map.Svg.selectAll(".label")
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
                d.width = map.ComputedTextLength(map, node.name) + 10;
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
                d.name = node.name;
                return d.name;
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

MapD3.prototype.Redraw = function (map, transition) {
    if (map.IsMouseDown) return;
    (transition ? map.Vis.transition() : map.Vis).attr("transform", map.transform);
};

MapD3.prototype.OnMouseOver = function (map, node) {
    if (node === null) return;
};

MapD3.prototype.OnMouseOut = function (map, node) {
    if (node === null) return;
};

MapD3.prototype.OnMouseClick = function (map, node) {
    if (node === null) return;
    map.OnNodeClick(node);
};

MapD3.prototype.ComputedTextLength = function (map, text) {
    var id = "ERTWERTWERTWERTWERTWE";

    var m = map.Svg
        .append("text")
        .attr("id", id)
        .attr("class", "d3label")
        .style("fill", "transparent");

    var length = m.text(text).node().getComputedTextLength();

    d3.select("#ERTWERTWERTWERTWERTWE").remove();

    return length;
};