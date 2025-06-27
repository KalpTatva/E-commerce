$(".loader3").show();
$('.listContainer').hide();

$(document).ready(function () {

    // signalR connection for real-time updates
    // hub connection 
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // start connection
    connection.start()
        .then(() => console.log("SignalR Connected"))
        .catch(err => console.error(err.toString()));

    // receive notification
    connection.on("ReceiveNotification", function (message) {
        //fetcch orders
        FetchOrders(currentPage, rowsPerPage);
    });

    $(document).on('click', '.PendingORderBtn', function () {
        var orderId = $(this).data('order-id');
        $.ajax({
            url: '/Order/UpdateOrderStatus',
            type: 'PUT',
            data: { orderId: orderId, status: 'Pending' },
            success: function (response) {
                if(response.success) {
                    toastr.success('Order status updated to Pending.');
                    FetchOrders(currentPage, rowsPerPage);
                } else {
                    toastr.error('Failed to update order status.');
                }
                
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while updating the order status.');
            }
        });
    });

    $(document).on('click', '.ShippedOrderBtn', function () {
        var orderId = $(this).data('order-id');
        $.ajax({
            url: '/Order/UpdateOrderStatus',
            type: 'PUT',
            data: { orderId: orderId, status: 'Shipped' },
            success: function (response) {
                if(response.success) {
                    toastr.success('Order status updated to Shipped.');
                    FetchOrders(currentPage, rowsPerPage);
                } else {
                    toastr.error('Failed to update order status.');
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while updating the order status.');
            }
        });
    });

    $(document).on('click', '.DeliveredOrderBtn', function () {
        var orderId = $(this).data('order-id');
        $.ajax({
            url: '/Order/UpdateOrderStatus',
            type: 'PUT',
            data: { orderId: orderId, status: 'Delivered' },
            success: function (response) {
                if(response.success) {
                    toastr.success('Order status updated to Delivered.');
                    FetchOrders(currentPage, rowsPerPage);
                } else {
                    toastr.error('Failed to update order status.');
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while updating the order status.');
            }
        });
    });

    FetchOrders(currentPage, rowsPerPage);




    var rowsPerPage = 5;
    var currentPage = 1;
    var totalItems = 0;

    function FetchOrders(page, pageSize) {
        $.ajax({
            url: '/Dashboard/GetSellerOrders',
            type: 'GET',
            data:{
                pageNumber: page,
                pageSize: pageSize
            },
            success: function (response) {
                $(".loader3").hide();
                $('#OrderContainer').html(response);
                totalItems = parseInt($("#TableContainer").attr("data-total-items")) || 0;
                updatePagination();
                $('.listContainer').show();
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while fetching the orders.');
            }
        });
    }



    // Update pagination info
  function updatePagination() {
    var totalPages = Math.ceil(totalItems / rowsPerPage);
    var startItem = (currentPage - 1) * rowsPerPage + 1;
    var endItem = Math.min(currentPage * rowsPerPage, totalItems);
    $("#pagination-info").text(
      `Showing ${startItem}-${endItem} of ${totalItems}`
    );
    $("#prevPage").toggleClass("disabled", currentPage === 1);
    $("#nextPage").toggleClass("disabled", currentPage >= totalPages);
  }

  // Page size change
  $(document).on("click", ".page-size-option", function (e) {
    e.preventDefault();
    var newSize = parseInt($(this).data("size"));
    if (newSize !== rowsPerPage) {
      rowsPerPage = newSize;
      $("#itemsPerPageBtn").html(
        `${rowsPerPage} <span><i class="bi bi-chevron-down"></i></span>`
      );
      currentPage = 1;
      FetchOrders(currentPage, rowsPerPage);
    }
    $("#itemsPerPageMenu").hide();
  });

  // Hide dropdown when clicking outside
  $(document).on("click", function (e) {
    if (!$(e.target).closest("#itemsPerPageBtn, #itemsPerPageMenu").length) {
      $("#itemsPerPageMenu").hide();
    }
  });

  // Toggle dropdown paging
  $("#itemsPerPageBtn").on("click", function () {
    $("#itemsPerPageMenu").toggle();
  });

  // Previous page
  $(document).on("click", "#prevPage", function (e) {
    e.preventDefault();
    if (currentPage > 1) {
      currentPage--;
      FetchOrders(currentPage, rowsPerPage);
    }
  });

  // Next page
  $(document).on("click", "#nextPage", function (e) {
    e.preventDefault();
    if (currentPage * rowsPerPage < totalItems) {
      currentPage++;
      FetchOrders(currentPage, rowsPerPage);
    }
  });
});