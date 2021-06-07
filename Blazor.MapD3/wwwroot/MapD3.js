﻿var _MapD3;
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
    var dw = _MapD3.Width * 0.2;
    var dh = _MapD3.Height * 0.15;
    var cw = Number(_MapD3.Width - dw);
    var ch = Number(_MapD3.Height - dh);
    var s = Math.min(cw / w, ch / h);
    var tx = dw / 2 + (-b.x * s + (cw / s - w) * s / 2);
    var ty = dh / 2 + (-b.y * s + (ch / s - h) * s / 2);

    MapD3ZoomTo(tx, ty, s);
}

export function MapD3ZoomToCenter(s) {
    var b = _MapD3.Bounds(_MapD3);
    var w = b.X - b.x;
    var h = b.Y - b.y;
    var dw = _MapD3.Width * 0.2;
    var dh = _MapD3.Height * 0.15;
    var cw = Number(_MapD3.Width - dw);
    var ch = Number(_MapD3.Height - dh);
    var tx = dw / 2 + (-b.x * s + (cw / s - w) * s / 2);
    var ty = dh / 2 + (-b.y * s + (ch / s - h) * s / 2);

    MapD3ZoomTo(tx, ty, s);
}


export function MapD3ZoomTo(x, y, s) {
    var transform = d3.zoomIdentity.translate(x, y).scale(s);

    _MapD3.Svg.call(_MapD3.Zoom.transform, transform);
}

function MapD3OnInternalNodeClick(e) {
    var node = NewNodeFromNode(e);

    _MapD3Instance.invokeMethodAsync('OnInternalNodeClick', node);
}

function NewNodeFromNode(e) {
    return {
        index: e.index,
        code: e.code,
        label: e.label,
        parents: e.parents,
        group: e.group,
        tooltip: e.tooltip,
        color: e.color,
        header: e.header,
        footer: e.footer,
        data: e.data
    };    
}

function NewLinkFromLink(e) {
    return {
        code: e.code,
        source: e.source,
        target: e.target,
        selected: e.selected
    };
}

function NewGroupFromGroup(e) {
    return {
        index: e.index,
        code: e.code,
        leaves: e.leaves,
        groups: e.groups,
        color: e.color
    };
}

function FillGroup(e, group) {
    group.index = e.index;
    group.leaves = e.leaves;
    group.groups = e.groups;
    group.color = e.color;
}

function FillLink(e, link) {
    link.selected = e.selected;
}

function FillNode(e, node) {
    node.index = e.index;
    node.label = e.label;
    node.parents = e.parents;
    node.group = e.group;
    node.tooltip = e.tooltip;
    node.color = e.color;
    node.header = e.header;
    node.footer = e.footer;
    node.data = e.data;
}

/* -----------------------------------------------------------------------------------------------------------------------------  */

function MapD3(width, height, div, action, onNodeClick) {
    this.Div = div;
    this.IsMouseDown = false;
    this.Action = action;
    this.OnNodeClick = onNodeClick;

    this.Width = document.getElementById(div).clientWidth;
    this.Height = document.getElementById(div).clientHeight;

    //alert("w: " + this.Width + " h:" + this.Height)

    this.Cola = cola.d3adaptor(d3)
        .avoidOverlaps(true)
        .handleDisconnected(true)
        .linkDistance(60)
        .size([this.Width, this.Height]);

    this.Svg = d3.select('#' + div)
        .append("svg")
        .attr("width", "100%")
        .attr("height", "100%")
        .attr("pointer-eventes", "all");

    //this.Svg.append('rect')
    //    .attr('width', "100%")
    //    .attr('height', "100%")
    //    .style('fill', 'white');

    this.Layers = this.Svg.append("g");

    this.GroupLayer = this.Layers.append("g");
    this.LinkLayer = this.Layers.append("g");
    this.NodeLayer = this.Layers.append("g");

    this.Links = [];
    this.Nodes = [];
    this.Groups = [];

    this.Zoom = d3.zoom()
        .extent([[0, 0], [this.Width, this.Height]])
        .scaleExtent([0.1, 10])
        .on("zoom", function (evt) {
            //map.Layers.attr("transform", evt.transform);
            _MapD3.Layers.attr("transform", "translate(" + evt.transform.x + "," + evt.transform.y + ") scale(" + evt.transform.k + ")");

        });

    this.Svg.call(this.Zoom);
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

    this.Width = document.getElementById(this.Div).clientWidth;
    this.Height = document.getElementById(this.Div).clientHeight;
    
    if (graph == null) {
        map.GroupLayer.selectAll("*").remove();
        map.LinkLayer.selectAll("*").remove();
        map.NodeLayer.selectAll("*").remove();
        return;
    }

    graph.groups.forEach(x => {
        var found = false;
        map.Groups.forEach(y => { if (x.code === y.code) found = true; FillGroup(x, y); });
        if (!found) map.Groups.push(NewGroupFromGroup(x));
    });

    map.Groups = map.Groups.filter(function (x) {
        var found = false;
        graph.groups.forEach(y => { if (x.code === y.code) found = true; });
        return found;
    });

    graph.links.forEach(x => {
        var found = false;
        map.Links.forEach(y => { if (x.code === y.code) found = true; FillLink(x, y); });
        if (!found) map.Links.push(NewLinkFromLink(x));
    });

    map.Links = map.Links.filter(function(x) {
        var found = false;
        graph.links.forEach(y => { if (x.code === y.code) found = true; });
        return found;
    });

    graph.nodes.forEach(x => {
        var found = false;
        map.Nodes.forEach(y => { if (x.code === y.code) { found = true; FillNode(x, y); } });
        if (!found) map.Nodes.push(NewNodeFromNode(x));
    });

    map.Nodes = map.Nodes.filter(function (x) {
        var found = false;
        graph.nodes.forEach(y => { if (x.code === y.code) found = true; });
        return found;
    });

    map.Cola
        .groups(map.Groups)
        .links(map.Links)
        .nodes(map.Nodes)
        .start();

    var group = map.GroupLayer.selectAll(".d3group")
        .data(map.Groups, d => d.code)
        .join("rect")
        .attr("rx", 5).attr("ry", 5)
        .attr("class", "d3group")
        .style("fill", d => d.color);

    var link = map.LinkLayer.selectAll(".d3link")
        .data(map.Links, d => d.code)
        .join("line")
        .attr("class", "d3link");   

    var node = map.NodeLayer.selectAll(".d3node")
        .data(map.Nodes, d => d.code)
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
            d.width = map.TextWidth(map, d.label, "d3label") + 10;
            return d.width;
        })
        .attr("height", function (d) {
            d.height = map.TextHeight(map, d.label, "d3label") + 4;
            return d.height;
        })
        .attr("rx", 5).attr("ry", 5)
        .style("fill", d => d.color)
        .on("click", function (evt, node) {
            map.OnMouseClick(evt, map, node);
        });

    node
        .append("title").attr("class", "d3tooltip")
        .text(d => d.tooltip);

    node
        .append("text").attr("class", "d3label")
        .attr("x", d => d.width / 2)
        .attr("y", d => d.height / 2 + 2)
        .text(d => d.label);

    node.filter(function (d) { return d.header !== null; })
        .append("text").attr("class", "d3labelHeader")
        .attr("x", d => d.width / 2)
        .attr("y", d => d.height / 2 -8)
        .text(d => d.header);

    node.filter(function (d) { return d.footer !== null; })
        .append("text").attr("class", "d3labelFooter")
        .attr("x", d => d.width / 2)
        .attr("y", d => d.height / 2 + 12)
        .text(d => d.footer);
           
    map.Cola.on("tick", function () {
        group
            .attr("x", d => d.bounds.x -3)
            .attr("y", d => d.bounds.y -3)
            .attr("width", d => d.bounds.width() +6)
            .attr("height", d => d.bounds.height() +6);

        link
            .attr("x1", d => d.source.x)
            .attr("y1", d => d.source.y)
            .attr("x2", d => d.target.x)
            .attr("y2", d => d.target.y);

        node
            .attr("transform", d =>  "translate(" + (d.x - d.width / 2) + "," + (d.y - d.height / 2) + ")");
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

MapD3.prototype.TextWidth = function (map, text, cssClass) {
    var id = "ERTWERTWERTWERTWERTWEW";

    var m = map.NodeLayer
        .append("text")
        .attr("id", id)
        .attr("class", cssClass) // "d3label")
        .style("fill", "transparent");

    var box = m.text(text).node().getBBox();
    var width = box.width;

    d3.select("#ERTWERTWERTWERTWERTWEW").remove();

    return width;
};

MapD3.prototype.TextHeight = function (map, text, cssClass) {
    var id = "ERTWERTWERTWERTWERTWEhH";

    var m = map.NodeLayer
        .append("text")
        .attr("id", id)
        .attr("class", cssClass) // "d3label")
        .style("fill", "transparent");

    var box = m.text(text).node().getBBox();
    var height = box.height;

    d3.select("#ERTWERTWERTWERTWERTWEH").remove();

    return height;
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