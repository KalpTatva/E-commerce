$(".loader3").hide();
$('.listContainer1').hide();

$(document).ready(function(){

    var deleteProductModal = new bootstrap.Modal(document.getElementById('ProductDeleteModal'), {
        keyboard: false,
        backdrop: 'static'
    });
    var totalItems = 0;
    // Get current page and rows per page from local storage
    currentPage = parseInt(localStorage.getItem('currentPage')) || 1;
    rowsPerPage = parseInt(localStorage.getItem('rowsPerPage')) || 5;

    function FetchProductsDetails(page, pageSize)
    {
        $(".loader3").show();
        $('.listContainer1').hide();
        $.ajax({
            url: '/Product/GetSellerSpecificProducts',
            type: 'GET',
            data: {
                pageNumber : page,
                pageSize: pageSize
            },
            success: function (response) {
                $(".loader3").hide();
                $("#productDetails").html(response);
                totalItems = parseInt($("#TableContainer").attr("data-total-items")) || 0;
                updatePagination();
                $('.listContainer1').show();
            },
            error: function () {
                toastr.error('An error occurred while loading the product.');
            }
        });
    }

    $(document).on('click','.product-delete-btn',function(){
        var product = $(this).data('product-id');
        $('#deleteProductId').val(product);
        deleteProductModal.show();
    });

    $(document).on('submit','#productDeleteForm', function(e){
        e.preventDefault();
        $.ajax({
            url: '/Product/DeleteProduct',
            type: 'PUT',
            data: $(this).serialize(),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message,"Success",{timeOut:5000});
                    FetchProductsDetails();
                    deleteProductModal.hide();
                } else {
                    toastr.error(response.message,"Error",{timeOut:5000});
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while delete the product.');
            }
        })
    })

    // Update pagination info
    function updatePagination() {
        if(totalItems == 0){
            localStorage.setItem('currentPage', 1);
            localStorage.setItem('rowsPerPage', 5);
            currentPage = 1;
            rowsPerPage = 5;
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
            FetchProductsDetails(currentPage, rowsPerPage);
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
            FetchProductsDetails(currentPage, rowsPerPage);
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
            FetchProductsDetails(currentPage, rowsPerPage);
            // upadate local storage
            localStorage.setItem('currentPage', currentPage);
            localStorage.setItem('rowsPerPage', rowsPerPage);
        }
    });
    FetchProductsDetails(currentPage, rowsPerPage);
});