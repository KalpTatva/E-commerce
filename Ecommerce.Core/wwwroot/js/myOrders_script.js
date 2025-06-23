$(".loader3").show();
$('#OrderContainer').hide();

$(document).ready(function () {
    
    function FetchOrders() {
        $.ajax({
            url: '/Dashboard/GetMyOrders',
            type: 'GET',
            success: function (response) {
                $(".loader3").hide();
                $('#OrderContainer').html(response);
                $('#OrderContainer').show();
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while delete the product.');
            }
        })
    }

    $(document).on('click','.CancelOrderBtn', function () {
        var orderId = $(this).data('order-id');
        $.ajax({
            url: '/Order/UpdateOrderStatus',
            type: 'PUT',
            data: { orderId: orderId, status: 'Cancelled'},
            success: function (response) {
                if(response.success) {
                    toastr.success('Order status updated to Cancelled.');
                    FetchOrders();
                } else {
                    toastr.error('Failed to cancel the order.');
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while cancelling the order.');
            }
        });
    });

    FetchOrders();
});