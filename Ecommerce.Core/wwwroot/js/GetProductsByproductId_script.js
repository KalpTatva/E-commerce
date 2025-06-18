$(document).ready(function () {
    $("#owl-demo").owlCarousel({
        items: 1, // Show one item at a time
        loop: true,
        //nav: true, // Next/Prev arrows
        dots: true, // Dots navigation
        autoplay: true,
        autoplayTimeout: 3000, // 3000ms = 3s delay
        autoplayHoverPause: true, // Pause on hover
        // navText: ["<", ">"], // You can customize the arrow text/icons
    });

    $(document).on('click', "#addTofavourite", function(){
        var $this = $(this);
        var id = $this.data("product-id");
        var isFavourite = $this.data("is-favourite"); // boolean

        $.ajax({
            url: '/BuyerDashboard/UpdateFavourite',
            type: 'POST',
            data: { productId: id },
            success: function (response) {
                if (response.success) {
                    toastr.success("Changes made successfully!", "Success", { timeOut: 4000 });

                    // Toggle the icon
                    var $icon = $this.find("i");
                    if ($icon.hasClass("bi-heart-fill")) {
                        $icon.removeClass("bi-heart-fill").addClass("bi-heart");
                        $this.data("is-favourite", false);
                    } else {
                        $icon.removeClass("bi-heart").addClass("bi-heart-fill");
                        $this.data("is-favourite", true);
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

});