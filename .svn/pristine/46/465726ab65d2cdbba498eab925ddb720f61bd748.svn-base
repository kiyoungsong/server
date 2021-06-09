function GenerateLineChart(jArray) {
    am4core.useTheme(am4themes_animated);
    var chart = am4core.create("chartdiv", am4charts.XYChart);
    var person = JSON.parse(jArray);

    chart.data = person;
    //time tag1 tag2 tag3
    let tags = Object.keys(person[person.length - 1]);

    // Create category axis
    var categoryAxis = chart.xAxes.push(new am4charts.CategoryAxis());
    categoryAxis.dataFields.category = "time";
    categoryAxis.renderer.opposite = true;

    // Create value axis
    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis.renderer.inversed = true;
    valueAxis.title.text = "Value";
    valueAxis.renderer.minLabelPosition = 0.01;

    // Create series
    // i : 0 key 1, 1:2, 2:3,
    var series = [];
    var hs = [];

    for (var i = 0; i < 10; i++) {
        if (i == (tags.length - 1)) {
            break;
        }
        var series1 = chart.series.push(new am4charts.LineSeries());
        series1.dataFields.valueY = tags[i + 1];
        series1.dataFields.categoryX = "time";
        series1.name = tags[i + 1];
        series1.bullets.push(new am4charts.CircleBullet());
        series1.tooltipText = "Place taken by {name} in {categoryX}: {valueY}";
        series1.legendSettings.valueText = "{valueY}";
        series1.visible = true;
        series[i] = series1;
    }

    chart.cursor = new am4charts.XYCursor();
    chart.cursor.behavior = "zoomY";

    for (var i = 0; i < 10; i++) {
        if (i == (tags.length - 1)) {
            break;
        }

        let hs1 = series[i].segments.template.states.create("hover")
        hs1.properties.strokeWidth = 5;
        series[i].segments.template.strokeWidth = 1;
        hs[i] = hs1;
    }
    
    // Add legend
    chart.legend = new am4charts.Legend();
    chart.legend.itemContainers.template.events.on("over", function (event) {
        var segments = event.target.dataItem.dataContext.segments;
        segments.each(function (segment) {
            segment.isHover = true;
        })
    })

    chart.legend.itemContainers.template.events.on("out", function (event) {
        var segments = event.target.dataItem.dataContext.segments;
        segments.each(function (segment) {
            segment.isHover = false;
        })
    })
}