$(".loader3").show();
$('#OrderContainer').hide();

$(document).ready(function () {
    var totalItems = 0;
    currentPage = parseInt(localStorage.getItem('currentPage')) || 1;
    rowsPerPage = parseInt(localStorage.getItem('rowsPerPage')) || 2;

    function FetchOrders(page, pageSize) {
        $(".loader3").show();
        $('#OrderContainer').hide();
        $.ajax({
            url: '/Dashboard/GetMyOrders',
            type: 'GET',
            data: {
                page: page || currentPage,
                pageSize: pageSize || rowsPerPage
            },
            success: function (response) {
                $(".loader3").hide();
                $('#OrderContainer').html(response);
                totalItems = parseInt($("#TableContainer").attr("data-total-items")) || 0;
                updatePagination();
                $('#OrderContainer').show();
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while delete the product.');
            }
        })
    }

    var CancelOrderModal = new bootstrap.Modal(document.getElementById('CancelOrderModal'), {
        keyboard: false,
        backdrop: 'static'
    });
    $(document).on('click','.CancelOrderBtn', function () {

        var orderProductId = $(this).data('order-id');
        $('#OrderProductIdInput').val(orderProductId);
        CancelOrderModal.show();
        
    });

    $(document).on('submit', '#CancelOrderForm', function (e) {
        e.preventDefault();
        var orderId = $('#OrderProductIdInput').val();
        $.ajax({
            url: '/Order/UpdateOrderStatus',
            type: 'PUT',
            data: { orderId: orderId, status: 'Cancelled'},
            success: function (response) {
                if(response.success) {
                    CancelOrderModal.hide();
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
    })


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
                    $('#AddReviewForm')[0].reset(); // Reset the form
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

    // Handle star click
    $(document).on("click", ".star-rating .star", function () {
        var $star = $(this);
        var value = parseInt($star.data("value"));
        var field = $star.closest(".star-rating").data("field");

        $('#RatingInput').val(value);
    
        // Update star visuals
        $star.closest(".star-rating").find(".star").each(function () {
            var starValue = parseInt($(this).data("value"));
            if (starValue <= value) {
                $(this).removeClass("bi-star").addClass("bi-star-fill").css("color", "#ffc107");
            } else {
                $(this).removeClass("bi-star-fill").addClass("bi-star").css("color", "");
            }
        });
    });



    // Update pagination info
    function updatePagination() {
        if(totalItems == 0){
            localStorage.setItem('currentPage', 1);
            localStorage.setItem('rowsPerPage', 2);
            currentPage = 1;
            rowsPerPage = 2;
        } 
        var totalPages = Math.ceil(totalItems / rowsPerPage);
        var startItem = (currentPage - 1) * rowsPerPage + 1;
        var endItem = Math.min(currentPage * rowsPerPage, totalItems);

        $("#pagination-info").text(
        `Showing ${startItem}-${endItem} of ${totalItems}`
        );
        $("#itemsPerPageBtn").html(
            `${rowsPerPage} <span><i class="bi bi-chevron-down"></i></span>`
        );
        $(".currentPage").html(`${currentPage}`);
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
            // get current page from local storage
            currentPage = parseInt(localStorage.getItem('currentPage')) || 1;

            // set row per page in local storage
            localStorage.setItem('rowsPerPage', rowsPerPage);
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
            // upadate local storage
            localStorage.setItem('currentPage', currentPage);
            localStorage.setItem('rowsPerPage', rowsPerPage);
        }
    });

    // Next page
    $(document).on("click", "#nextPage", function (e) {
        e.preventDefault();
        if (currentPage * rowsPerPage < totalItems) {
            currentPage++;
            FetchOrders(currentPage, rowsPerPage);
            // upadate local storage
            localStorage.setItem('currentPage', currentPage);
            localStorage.setItem('rowsPerPage', rowsPerPage);
        }
    });

    FetchOrders(currentPage, rowsPerPage);
});