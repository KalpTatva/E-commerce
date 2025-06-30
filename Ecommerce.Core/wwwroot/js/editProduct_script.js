$(document).ready(function () {

    let ImageIdToDelete = [];

    $(document).on('click', '#CategorySelect', function () {
        $('#selectedCategory').remove();
    });

    // jq for adding inputs to add new features
    $('#AddFeatureButton').on('click', function () {
        const featureItem = `
            <div class="feature-item my-3 gap-3 d-flex justify-content-between">
                <div class="form-floating w-100">
                    <input type="text" class="form-control feature-name" id="FeatureName" placeholder="Feature Name">
                    <label for="FeatureName">Feature Name</label>
                </div>
                <div class="form-floating w-100">
                    <input type="text" class="form-control feature-description" id="FeatureDescription" placeholder="Feature Description">
                    <label for="FeatureDescription">Feature Description</label>
                </div>
                <button type="button" class="btn btn-danger remove-feature-button"><i class="bi bi-x-lg"></i></button>
            </div>`;
        $('.features-container').append(featureItem);
    });

    // jq which handle removing feature input
    $(document).on('click', '.remove-feature-button', function () {
        $(this).closest('.feature-item').remove();
    });

    // jq for adding details of features before submitting edit form
    $('#EditProductForm').on('submit', function () {
        const features = [];
        $('.feature-item').each(function () {
            const featureName = $(this).find('.feature-name').val();
            const featureDescription = $(this).find('.feature-description').val();
            if (featureName && featureDescription) {
                features.push({ FeatureName: featureName, Description: featureDescription });
            }
        });
        $('#FeaturesInput').val(JSON.stringify(features));
        $("#ImageDeleteInput").val(JSON.stringify(ImageIdToDelete));
        console.log(features);
        console.log(ImageIdToDelete);
    });


    // jq for handling image

    $(document).on('click', '.removeImageBtn', function () {
        console.log($(this).data("image-id"));
        ImageIdToDelete.push($(this).data("image-id"));
        console.log('Array is : ', ImageIdToDelete);
        $(this).closest('.img-wrapper').remove();
    });


    // mthod for image showcasing
    $("#ProductImages").on("change", function (event) {
        const files = event.target.files;
        const previewContainer = $(".image-preview-container");

        $("#ProductImages").after(previewContainer);

        const existingImages = new Set();

        Array.from(files).forEach((file) => {
        
            // Validate file type and check for duplicates
            const validExtensions = ["jpg", "jpeg", "avif","png","svg"];
            const fileExtension = file.name.split(".").pop().toLowerCase();
            if (!validExtensions.includes(fileExtension)) {
                toastr.error(`File "${file.name}" is not a valid image format. Allowed formats: jpg, jpeg, avif, svg, png.`);
                return;
            }
            
            // Check if the image is already added
            if (existingImages.has(file.name)) {
                toastr.error(`Image "${file.name}" is already added.`);
                return;
            }
            existingImages.add(file.name);
        
            const reader = new FileReader();
            reader.onload = function (e) {
                const imgWrapper = $(
                    '<div class="img-wrapper d-inline-block position-relative m-2"></div>'
                );
                const img = $(
                    '<img class="img-thumbnail" style="width: 70px; height: 70px;">'
                );
                img.attr("src", e.target.result);
        
                const removeBtn = $(
                    '<button class="btn btn-danger btn-sm position-absolute top-0 end-0">X</button>'
                );
                removeBtn.on("click", function () {
                    imgWrapper.remove();
                });
        
                imgWrapper.append(img).append(removeBtn);
                previewContainer.append(imgWrapper);
            };
            reader.readAsDataURL(file);
        });
    });


    // summer note
    $('#Description').summernote({
        height: 200,
        placeholder: 'Type your description here...it should be detailed and informative which can make impact on customer',
        toolbar: [
          ['style', ['bold', 'italic', 'underline', 'clear']],
          ['font', ['strikethrough', 'superscript', 'subscript']],
          ['fontsize', ['fontsize']],
          ['color', ['color']],
          ['para', ['ul', 'ol', 'paragraph']],
          ['height', ['height']]
        ]
    });
});
