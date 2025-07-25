$('.RevenueNotFound').hide();
$('.TopSellingProductNotFound').hide();
$('.CustomersNotFound').hide();
$('.RevenueNotFound').hide();
$('.TopSellingProductNotFound').hide();
$('.LeastSellingProductNotFound').hide();


$(document).ready(function () {
    Chart.defaults.global.defaultFontColor = 'rgb(139, 139, 139)';
    var selector;
    var openCustomeDateModal = new bootstrap.Modal(document.getElementById('CustomDates'), {backdrop: 'static', keyboard: false});
    
    // Get current date (today) in YYYY-MM-DD format
    var today = new Date().toISOString().split("T")[0];
    // Set max attribute for date inputs
    $("#ToDateInput").attr("max", today);
    $("#FromDateInput").attr("max", today);

    function validateDates() {
        var fromDateVal = $("#FromDateInput").val();
        var toDateVal = $("#ToDateInput").val();
        var isValid = true;
    
        $("#fromDateError").text("");
        $("#toDateError").text("");
    
        if (fromDateVal && toDateVal) {
            if (new Date(fromDateVal) > new Date(toDateVal)) {
                $("#fromDateError").text("From date can not be greater than To date");
                isValid = false;
            }
        }
        if (toDateVal && new Date(toDateVal) > new Date(today)) {
            $("#toDateError").text("To Date cannot be greater than Today");
            isValid = false;
        }
        return isValid;
    }


    function GetDashboardData(selectorValue, fromDate, toDate) {
        $.ajax({
            url: '/Dashboard/GetDashBoardData',
            type: 'GET',
            data: { 
                selector: selectorValue,
                fromDate: fromDate,
                toDate: toDate
            },
            success: function (data) {
                // Update the charts and items with the received data
                console.log(data);
                setData(data.data);
            },
            error: function (error) {
                console.error("Error fetching dashboard data:", error);
            }
        });
    }

    function setData(data) {
        if(data.priceAndDate.length === 0) {  
            $('.RevenueNotFound').show();
            $('#RevenueChart').hide();
        }else {
            $('.RevenueNotFound').hide();
            $('#RevenueChart').show();
            generateRevenueChart(data.priceAndDate);
        }
        if(data.customersData.length === 0) {
            $('.CustomersNotFound').show();
            $('#CustomersChart').hide();
        }else {
            $('.CustomersNotFound').hide();
            $('#CustomersChart').show();
            generateCustomersChart(data.customersData);
        }
        if(data.topSellingProduct.length === 0) {
            $('.TopSellingProductNotFound').show();
            $('#topItems').hide();
        }else {
            $('.TopSellingProductNotFound').hide();
            fetchItems(data.topSellingProduct, "#topItems");
            generateTopSellingProductChart(data.topSellingProduct);
            $('#topItems').show();
        }   
        if(data.leastSellingProduct.length === 0) {
            $('.LeastSellingProductNotFound').show();
            $('#lastItems').hide();
        }
        else {
            $('.LeastSellingProductNotFound').hide();
            fetchItems(data.leastSellingProduct, "#lastItems");
            generateLeastSellingProductChart(data.leastSellingProduct);
            $('#lastItems').show();
        }

    }

    function fetchItems(items, elementId) {
        var itemsHtml = "";
        var count = 1;
        itemsHtml += "<ul>";
        $.each(items, function (index, item) {
          itemsHtml += `
                <li class="d-flex flex-row p-3">
                    <div class="d-flex flex-row justify-content-start align-items-center w-100">
                        <p class="me-3">${count}</p>
                        <img class="border border-1 rounded-circle my-auto ms-2" src="${item.imageUrl}" height="70" width="70" alt="${item.productName}">
                        <div class="pt-2 d-flex flex-column justify-content-start align-items-start ms-3">
                            <h5 class="mb-1">${item.productName}</h5>
                            <h5 class="mb-0"><i class="bi bi-pc-display-horizontal"></i> ${item.count}</h5>
                        </div>
                    </div>
                </li>
            `;
    
          count++;
        });
        itemsHtml += "</ul>";
        $(elementId).html(itemsHtml);
    }
    
    function generateRevenueChart(data) {
        const dateNumbers = data.map(i => i.dateNumber);
        const revenues = data.map(i => i.price);
        chartGenerator("RevenueChart", dateNumbers, revenues, "Revenue");
    }
    function generateCustomersChart(data) {
        const dateNumbers = data.map(i => i.dateNumber);
        const customers = data.map(i => i.customerCount);
        chartGenerator("CustomerChart", dateNumbers, customers, "Customers");
    }
    function generateTopSellingProductChart(data) {
        const productNames = data.map(i => i.productName.substring(0, 30) + "...");
        const productCounts = data.map(i => i.count);
        horizontalChart("TopSellingProductChart", productNames, productCounts, "Top Selling Product");
    }
    function generateLeastSellingProductChart(data) {
        const productNames = data.map(i => i.productName.substring(0, 30) + "...");
        const productCounts = data.map(i => i.count);
        horizontalChart("LeastSellingProductChart", productNames, productCounts, "Least Selling Product");
    }

    // function for generating charts
    function chartGenerator(chartFor, dateNumbers, revenues, labels) {

        new Chart(chartFor, {
            type: "line",
            data: {
                labels: dateNumbers,
                datasets: [
                    {
                        label: labels,
                        borderColor: "#4caf50",
                        data: revenues,
                        fill: true,
                        backgroundColor: "#4caf506b",
                    },
                ],
            },
            options: {
                legend: { display: true },
                scales: {
                    yAxes: [
                        {
                            ticks: {
                                beginAtZero: true,
                            },
                        },
                    ],
                },
            },
        });
    }

    function horizontalChart(chartFor, labels, data, label) {
        new Chart(chartFor, {
            type: "horizontalBar",
            data: {
                labels: labels,
                datasets: [
                    {
                        label: label,
                        backgroundColor: "#4caf50",
                        data: data,
                    },
                ],
            },
            options: {
                legend: { display: true },
                scales: {
                    xAxes: [
                        {
                            ticks: {
                                beginAtZero: true,
                            },
                        },
                    ],
                },
            },
        });
    }


    // Event handler for the selector change
    $(document).on('click','.selector' ,function () {
        selector = $(this).attr('data-value');
        var selectedText = $(this).text();
        $('#selectorText').text(selectedText);
        if(selector == 3)
        {
            openCustomeDateModal.show();
        }else{
            $('#CustomDateValues').text('');
            GetDashboardData(selector,null,null);
        }
    });

    $("#FromDateInput, #ToDateInput").on("change", function () {
        validateDates();
    });
    
    $(".cloasing").on("click", function () {
        $("#dateRangeFilter").val(1);
        $("#FromDateInput").val("");
        $("#ToDateInput").val("");
        fromDate = "";
        toDate = "";
    });

    // submittig custom date form
    $(document).on("click", "#customdateSubmit", function (e) {
        e.preventDefault();
        if (validateDates()) {
            var fromDate = $("#FromDateInput").val();
            var toDate = $("#ToDateInput").val();
            if(!fromDate) {
                $("#fromDateError").text("From date is required");
            }
            if(!toDate) {
                $("#toDateError").text("To date is required");
            }
            if(fromDate && toDate) {
                $('#CustomDateValues').text(`From : ${fromDate} To : ${toDate}`);
                GetDashboardData(selector, fromDate, toDate);
                openCustomeDateModal.hide();
            }
        } else {
            toastr.error("Invalid date selection", "Error", { timeOut: 3000 });
        }
    });

    GetDashboardData(1,null,null);

});