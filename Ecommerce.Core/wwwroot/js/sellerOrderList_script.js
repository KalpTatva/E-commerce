$(".loader3").show();
$('#OrderContainer').hide();

$(document).ready(function () {
    function FetchOrders() {
        $.ajax({
            url: '/Dashboard/GetSellerOrders',
            type: 'GET',
            success: function (response) {
                $(".loader3").hide();
                $('#OrderContainer').html(response);
                $('#OrderContainer').show();
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while fetching the orders.');
            }
        });
    }

    $(document).on('click', '.PendingORderBtn', function () {
        var orderId = $(this).data('order-id');
        $.ajax({
            url: '/Order/UpdateOrderStatus',
            type: 'PUT',
            data: { orderId: orderId, status: 'Pending' },
            success: function (response) {
                if(response.success) {
                    toastr.success('Order status updated to Pending.');
                    FetchOrders();
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
                    FetchOrders();
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
                    FetchOrders();
                } else {
                    toastr.error('Failed to update order status.');
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while updating the order status.');
            }
        });
    });

    FetchOrders();
});