$(".loader3").hide();

$(document).ready(function () {
    let categoryInput = new URLSearchParams(window.location.search).get('categoryId');
    let searchInput = new URLSearchParams(window.location.search).get('search');
    var currentPage = localStorage.getItem('currentPage') ? parseInt(localStorage.getItem('currentPage')) : 1;
    var rowsPerPage = localStorage.getItem('rowsPerPage') >= 25 ? parseInt(localStorage.getItem('rowsPerPage')) : 25;
    
    // add search value into search input if exists
    if (searchInput != null && searchInput != "null") {
        $("#searchInput").val(searchInput);
    }

    // function for getting product on page
    function FetchProducts(categoryInput, searchInput, currentPage, rowsPerPage) {
        $(".loader3").show();

        categoryInput = new URLSearchParams(window.location.search).get('categoryId');
        $.ajax({
            url: '/BuyerDashboard/GetProducts',
            type: 'GET',
            data: {
                search: searchInput,
                category: categoryInput,
                page: currentPage,
                pageSize: rowsPerPage
            },
            success: function (response) {
               
                $("#ProductsContainer").html(response);
                totalItems = parseInt($("#TableContainer").attr("data-total-items")) || 0;
                updatePagination();
                $(".loader3").show();
            },
            error: function () {
                toastr.error('An error occurred while loading the product.');
            },
            complete: function () {
                $(".loader3").hide();
            }
        });
    };


    $(document).on('input','#searchInput',function(){
        searchInput = $(this).val();
        // add search value with url for search consistancy
        var url = new URL(window.location.href);

        if(categoryInput !== null) {
            url.searchParams.set('categoryId', categoryInput);
        }
        else {
            url.searchParams.delete('categoryId');
        }
        if (searchInput !== null) {
            url.searchParams.set('search', searchInput);
           
        } else {
            url.searchParams.delete('search');
        }
        window.history.pushState({}, '', url);

        currentPage = 1;
        rowsPerPage = 25;
        $("#ProductsContainer").empty();

        // debounce fetch products
        $(".loader3").show();
        clearTimeout($.data(this, 'timer'));
        $(this).data('timer', setTimeout(function () {    
            FetchProducts(categoryInput, searchInput, currentPage, rowsPerPage);
        }
        , 1000));
    });

    
    // Update pagination info
    function updatePagination() {
        if(totalItems == 0){
            localStorage.setItem('currentPage', 1);
            localStorage.setItem('rowsPerPage', 25);
            currentPage = 1;
            rowsPerPage = 25;
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
            localStorage.setItem('currentPage', 1);
            localStorage.setItem('rowsPerPage', rowsPerPage);
            currentPage = 1;
            rowsPerPage = rowsPerPage;
            FetchProducts(categoryInput, searchInput, currentPage, rowsPerPage);
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
        // scroll back to the top
        $('html, body').animate({ scrollTop: 0 }, 'fast');
        if (currentPage > 1) {
            currentPage--;
            FetchProducts(categoryInput, searchInput, currentPage, rowsPerPage);
            // upadate local storage
            localStorage.setItem('currentPage', currentPage);
            localStorage.setItem('rowsPerPage', rowsPerPage);
        }
    });

    // Next page
    $(document).on("click", "#nextPage", function (e) {
        e.preventDefault();
        // scroll back to the top
        $('html, body').animate({ scrollTop: 0 }, 'fast');
        if (currentPage * rowsPerPage < totalItems) {
            currentPage++;
            FetchProducts(categoryInput, searchInput, currentPage, rowsPerPage);
            // upadate local storage
            localStorage.setItem('currentPage', currentPage);
            localStorage.setItem('rowsPerPage', rowsPerPage);
        }
    });
    
    
    FetchProducts(categoryInput, searchInput, currentPage, rowsPerPage);



    // for redirection to the selected product
    $(document).on('click', '.card-img', function () {
        var product = $(this).data("product-id");
        window.location.href = '/BuyerDashboard/GetProductsByproductId?productId=' + product;
    });

    $(document).on('click', ".addTofavourite", function () {
        var $this = $(this);
        var id = $this.data("product-id");
        var $icon = $this.find("i");

        $.ajax({
            url: '/BuyerDashboard/UpdateFavourite',
            type: 'POST',
            data: { productId: id },
            success: function (response) {
                if (response.success) {
                    toastr.success("Changes made successfully!", "Success", { timeOut: 4000 });

                    if ($icon.hasClass("bi-heart-fill")) {
                        $icon.removeClass("bi-heart-fill").addClass("bi-heart");
                        $this.attr("data-is-favourite", "false");
                    } else {
                        $icon.removeClass("bi-heart").addClass("bi-heart-fill");
                        $this.attr("data-is-favourite", "true");
                    }
                } else {
                    toastr.error('An error occurred while updating favourite.', "Error", { timeOut: 4000 });
                }
            },
            error: function () {
                toastr.error('An error occurred while updating favourite.', "Error", { timeOut: 4000 });
            }
        });
    });

    $(document).on('click', '.AddToCart', function (e) {
        e.preventDefault();
        var productId = $(this).data("product-id");
        $.ajax({
            url: '/BuyerDashboard/AddToCart',
            type: 'POST',
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    window.location.href = '/BuyerDashboard/Cart';
                }
                else {
                    toastr.error(response.message, "Error", { timeOut: 4000 });
                }
            },
            error: function () {
                toastr.error('An error occurred while updating cart', "Error", { timeOut: 4000 });
            }
        })
    })


    $(document).on('click','.BuySingleProduct',function(){
        var productId = $(this).data('product-id');
        
        var arrayHelper = [];
        arrayHelper.push(productId);
        
        var obj = {
            orders: arrayHelper,
            totalPrice: $(this).data('price'),
            totalDiscount: $(this).data('discount'),
            totalQuantity:$(this).data('quantity'),
            isByProductId: true,
        };

        $.ajax({
            url:"/Order/SetSessionForOrder",
            type:"POST",
            data:{objectCart:JSON.stringify(obj)},
            success:function(data){
                if(data.success)
                {
                    window.location.href = '/Order/BuyProduct?sessionId='+data.message;
                }
                else
                {
                    toastr.error('An error occurred while setting up order', "Error", { timeOut: 4000 });
                }
            },error: function(){
                toastr.error('An error occurred while setting up order', "Error", { timeOut: 4000 });
            }
        })

    });







});