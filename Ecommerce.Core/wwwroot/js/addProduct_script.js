$(document).ready(function () {

    $(document).on('submit','#AddProductForm',function(e){
        e.preventDefault();
        $.ajax({
            url: '/Product/AddProduct',
            type: 'POST',
            data: new FormData(this),
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    setTimeout(() => {
                        window.location.href = '/Product/MyProducts';
                    }, 1500);
                } else {
                    toastr.error(response.message);
                }
            },
            error: function (xhr, status, error) {
                toastr.error('An error occurred while adding the product.');
            }
        });
    })

    // mthod for image showcasing
    $('#ProductImages').on('change', function (event) {
        
        const files = event.target.files;
        const previewContainer = $('.image-preview-container');
        
        $('#ProductImages').after(previewContainer);
        
        const existingImages = new Set();

        Array.from(files).forEach((file) => {
            if (existingImages.has(file.name)) {
                toastr.error(`Image "${file.name}" is already added.`);
                return;
            }
            existingImages.add(file.name);
        });
        
        Array.from(files).forEach((file, index) => {
            const reader = new FileReader();
            reader.onload = function (e) {
                const imgWrapper = $('<div class="img-wrapper d-inline-block position-relative m-2"></div>');
                const img = $('<img class="img-thumbnail" style="width: 100px; height: 100px;">');
                img.attr('src', e.target.result);

                const removeBtn = $('<button class="btn btn-danger btn-sm position-absolute top-0 end-0">X</button>');
                removeBtn.on('click', function () {
                    imgWrapper.remove();
                });

                imgWrapper.append(img).append(removeBtn);
                previewContainer.append(imgWrapper);
            };
            reader.readAsDataURL(file);
        });
    });
});