$(document).ready(function () {
    $(document).on("submit", "#AddProductForm", function (e) {
        e.preventDefault();
        $.ajax({
            url: "/Product/AddProduct",
            type: "POST",
            data: new FormData(this),
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    setTimeout(() => {
                        window.location.href = "/Product/MyProducts";
                    }, 1500);
                } else {
                    toastr.error(response.message);
                }
            },
            error: function (xhr, status, error) {
                toastr.error("An error occurred while adding the product.");
            },
        });
    });

    $("#ProductImages").on("change", function (event) {
        const files = event.target.files;
        const previewContainer = $(".image-preview-container");
        const validExtensions = ["jpg", "jpeg", "avif", "png", "svg", "bmp", "gif", "webp", "tiff", "heic", "ico", "raw", "jfif"];
        const existingImages = new Set();
        
        // Create a new FileList to store only valid files
        const validFiles = Array.from(files).filter((file) => {
            const fileExtension = file.name.split(".").pop().toLowerCase();
            if (!validExtensions.includes(fileExtension)) {
                toastr.error(`File "${file.name}" is not a valid image format.`);
                return false;
            }
            if (existingImages.has(file.name)) {
                toastr.error(`Image "${file.name}" is already added.`);
                return false;
            }
            existingImages.add(file.name);
            return true;
        });
    
        // Clear the input and set only valid files
        const dataTransfer = new DataTransfer();
        validFiles.forEach(file => dataTransfer.items.add(file));
        this.files = dataTransfer.files;
    
        // Move previewContainer after #ProductImages
        $("#ProductImages").after(previewContainer);
    
        // Process only valid files for preview
        validFiles.forEach((file) => {
            if (!file.type.startsWith("image/")) return;
    
            const reader = new FileReader();
            reader.onload = function (e) {
                const imgWrapper = $(
                    '<div class="img-wrapper d-inline-block position-relative m-2"></div>'
                );
                const img = $(
                    '<img class="img-thumbnail" style="width: 100px; height: 100px;">'
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

    // js for adding new features 
    $("#AddFeatureButton").on("click", function () {
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
        $(".features-container").append(featureItem);
    });

    // jq for removing feature
    $(document).on("click", ".remove-feature-button", function () {
        $(this).closest(".feature-item").remove();
    });

    // jq for adding values of features before submission
    $("#AddProductForm").on("submit", function () {
        const features = [];
        $(".feature-item").each(function () {
            const featureName = $(this).find(".feature-name").val();
            const featureDescription = $(this).find(".feature-description").val();
            if (featureName && featureDescription) {
                features.push({
                    FeatureName: featureName,
                    Description: featureDescription,
                });
            }
        });
        $("#FeaturesInput").val(JSON.stringify(features));
        console.log(features);
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