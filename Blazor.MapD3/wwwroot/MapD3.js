var _MapD3;
var _MapD3Instance;

export function MapD3Init(div, width, height, instance, service) {
    _MapD3Instance = instance;
    _MapD3 = new MapD3(width, height, div, service, MapD3OnInternalNodeClick);
    //if(service != null) _MapD3.UpdateService(_MapD3);
}

function MapD3UpdateService() {
    _MapD3.UpdateService(_MapD3);
}

export function MapD3Update(data) {
    _MapD3.Update(_MapD3, data);
}

export function MapD3ZoomToFit() {
    var b = _MapD3.Bounds(_MapD3);
    var w = b.X - b.x;
    var h = b.Y - b.y;
    var cw = Number(_MapD3.Width);
    var ch = Number(_MapD3.Height);
    var s = Math.min(cw / w, ch / h);
    var tx = (-b.x * s + (cw / s - w) * s / 2);
    var ty = (-b.y * s + (ch / s - h) * s / 2);

    var transform = d3.zoomIdentity.translate(tx, ty).scale(s);

    _MapD3.Layers.attr("transform", "translate(" + transform.x + "," + transform.y + ") scale(" + transform.k + ")");
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
        .linkDistance(60)
        .size([width, height]);

    this.Svg = d3.select('#' + div)
        .append("svg")
        .attr("width", "100%")
        .attr("height", height)
        .attr("pointer-eventes", "all")
        .attr("id", "svg-" + div);

    //this.Svg.append('rect')
    //    .attr('width', "100%")
    //    .attr('height', "100%")
    //    .style('fill', 'white');

    this.Layers = map.Svg.append("g");

    this.BaseLayer = map.Layers.append("g");
    this.NodeLayer = map.Layers.append("g");

    this.Zoom = d3.zoom()
        .extent([[0, 0], [width, height]])
        .scaleExtent([0.1, 10])
        .on("zoom", function (evt) {
            map.Layers.attr("transform", evt.transform);
        });

    this.Svg.call(map.Zoom);
}

MapD3.prototype.UpdateService = function (map) {
    //d3.json(map.Action, function (error, graph) {
    $.getJSON(map.Action, function (graph) {
        Update(map, graph);
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

MapD3.prototype.Update = function (map, graph) {
    if (graph == null) {
        map.BaseLayer.selectAll("*").remove();
        map.NodeLayer.selectAll("*").remove();
        return;
    }

    //map.NodeLayer.selectAll(".d3node")
    //    .data(graph.nodes, function (d) { return d.code; }).exit().remove();

    //map.BaseLayer.selectAll(".d3link")
    //    .data(graph.links, function (d) { return d.code; }).exit().remove();

    map.Cola
        .links(graph.links)
        .nodes(graph.nodes)
        .start();

    var link = map.BaseLayer.selectAll(".d3link")
        .data(graph.links, function (d) { return d.code; })
        .join("line")
        .attr("class", "d3link");

    var node = map.NodeLayer.selectAll(".d3node")
        .data(graph.nodes, function (d) { return d.code; })
        .join("g")
        .attr("class", "d3node")
        .on("touchmove", function () { d3.event.preventDefault() })
        .on("mouseenter", function (evt, node) {
            map.OnMouseOver(evt, map, node);
        })
        .on("mouseleave", function (evt, node) {
            map.OnMouseOut(evt, map, node);
        }) 
        .call(map.Cola.drag);

    node
        .append("rect")
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
        .on("click", function (evt, node) {
            map.OnMouseClick(evt, map, node);
        });

    node
        .append("title").attr("class", "d3tooltip")
        .text(function (d) { return d.tooltip; });

    node
        .append("text").attr("class", "d3label")
        .attr("x", function (d) { return d.width / 2; })
        .attr("y", function (d) { return d.height / 2 + 2; })
        .text(function (d) { return d.label; });

    node.filter(function (d) { return d.header !== null; })
        .append("text").attr("class", "d3labelHeader")
        .attr("x", function (d) { return d.width / 2; })
        .attr("y", function (d) { return d.height / 2 -8; })
        .text(function (d) { return d.header; });

    node.filter(function (d) { return d.footer !== null; })
        .append("text").attr("class", "d3labelFooter")
        .attr("x", function (d) { return d.width / 2; })
        .attr("y", function (d) { return d.height / 2 + 12; })
        .text(function (d) { return d.footer; });
           
    map.Cola.on("tick", function () {
        link
            .attr("x1", function (d) { return d.source.x; })
            .attr("y1", function (d) { return d.source.y; })
            .attr("x2", function (d) { return d.target.x; })
            .attr("y2", function (d) { return d.target.y; });

        node
            .attr("transform", function (d) { return "translate(" + (d.x - d.width / 2) + "," + (d.y - d.height / 2) + ")"; });
    });
};

MapD3.prototype.OnMouseOver = function (evt, map, node) {
    if (node === null) return;
};

MapD3.prototype.OnMouseOut = function (evt, map, node) {
    if (node === null) return;
};

MapD3.prototype.OnMouseClick = function (evt, map, node) {
    if (node === null) return;
    map.OnNodeClick(node);
};

MapD3.prototype.ComputedTextLength = function (map, text) {
    var id = "ERTWERTWERTWERTWERTWE";

    var m = map.NodeLayer
        .append("text")
        .attr("id", id)
        .attr("class", "d3label")
        .style("fill", "transparent");

    var length = m.text(text).node().getComputedTextLength();

    d3.select("#ERTWERTWERTWERTWERTWE").remove();

    return length;
};

MapD3.prototype.Bounds = function (map) {
    var x = Number.POSITIVE_INFINITY, X = Number.NEGATIVE_INFINITY, y = Number.POSITIVE_INFINITY, Y = Number.NEGATIVE_INFINITY;

    map.NodeLayer.selectAll(".d3node").each(function (v) {
        x = Math.min(x, v.x - v.width / 2);
        X = Math.max(X, v.x + v.width / 2);
        y = Math.min(y, v.y - v.height / 2);
        Y = Math.max(Y, v.y + v.height / 2);
    });
    return { x: x, X: X, y: y, Y: Y };
}