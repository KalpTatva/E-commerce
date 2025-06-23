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


    var AddReviewModal = new bootstrap.Modal(document.getElementById('AddReviewModal'), {
        keyboard: false,
        backdrop: 'static'
    });


    $(document).on('click', '.AddReviewBtn', function () {
        var orderproductId = $(this).data('order-id');
        var productId = $(this).data('product-id');
        AddReviewModal.show();
        $('#OrderProductId').val(orderproductId);
        $('#ProductId').val(productId);

    
    });

    $(document).on('submit', '#AddReviewForm', function (e) {
        e.preventDefault();
        var orderProductId = $('#OrderProductId').val();
        var productId = $('#ProductId').val();
        var rating = $('#RatingInput').val();
        var reviewText = $('#ReviewText').val();

        $.ajax({
            url: '/Order/AddReview',
            type: 'POST',
            data: {
                orderProductId: orderProductId,
                rating: rating,
                productId:productId,
                reviewText: reviewText
            },
            success: function (response) {
                if (response.success) {
                    toastr.success('Review added successfully.');
                    AddReviewModal.hide();
                    FetchOrders();
                } else {
                    toastr.error('Failed to add the review.');
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while adding the review.');
            }
        });
    });

    FetchOrders();
});