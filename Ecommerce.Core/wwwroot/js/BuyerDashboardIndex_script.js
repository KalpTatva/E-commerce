$(".loader3").hide();

$(document).ready(function () {
    let categoryInput= null;
    let searchInput = null;

    // signalR connection for real-time updates
    // hub connection 
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/NotificationHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    // start connection
    connection.start()
        .then(() => console.log("SignalR Connected"))
        .catch(err => console.error(err.toString()));

    // receive notification
    connection.on("ReceiveNotification", function (message) {
        //fetcch products 
        FetchProducts(categoryInput, searchInput);
    });

    var pageNumber = 0;
    var isFetching = false;
    var isReachedToBottom = false;
    var pendingRequests = 0; // Track the number of pending requests

    $(window).scroll(function () {
        if (!isReachedToBottom && $(window).scrollTop() + $(window).height() >= $(document).height()) {
            // fetch more products
            FetchProducts(categoryInput, searchInput);
        }
    });

    // function for getting product on page
    function FetchProducts(categoryInput, searchInput) {
        if (pageNumber > -1 && !isFetching) {
            isFetching = true; // Set fetching flag
            pageNumber++;
            $(".loader3").show();
            categoryInput = new URLSearchParams(window.location.search).get('categoryId');

            return $.ajax({
                url: '/BuyerDashboard/GetProducts',
                type: 'GET',
                data: {
                    search: searchInput,
                    category: categoryInput,
                    page: pageNumber
                }
            })
            .done(function (response) {
                $(".loader3").hide();
                $("#ProductsContainer").append(response);
            })
            .fail(function () {
                toastr.error('An error occurred while loading the product.');
            })
            .always(function () {
                isFetching = false; // Reset fetching flag
            });
        }
    }


    $(document).on('input','#searchInput',function(){
        searchInput = $(this).val();
        pageNumber = 0;
        $("#ProductsContainer").empty();
        FetchProducts(categoryInput, searchInput);
    });


    // Debounce function
    function debounce(func, delay) {
        let timer;
        return function (...args) {
            clearTimeout(timer);
            timer = setTimeout(() => func.apply(this, args), delay);    
        };
    }

    // Search input handler with debounce
    $(document).on('input', '#searchInput', debounce(function () {
        searchInput = $(this).val();
        pageNumber = 0; // Reset page number
        $("#ProductsContainer").empty(); // Clear existing products
        FetchProducts(categoryInput, searchInput);
    }, 1000)); 

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

    FetchProducts(categoryInput, searchInput);
});