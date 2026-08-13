/* Profile Edit (modal) - avatar crop + upload.
   Loaded globally; the modal-injected partial calls ProfileEdit.init(cfg). */

window.ProfileEdit = (function () {
    "use strict";

    return {
        init: function (cfg) {
            cfg = cfg || {};
            var L = cfg.labels || {};

            JSManager.initTrackableSections();

            var cropSize = 128;
            var cropper;
            var newImageData = null;
            var isChanged = false;

            function showSaveButton() { $("#saveAvatar").show(); }

            $("#avatarInput").off('change.profile').on("change.profile", function (e) {
                var file = e.target.files[0];
                if (!file) return;

                var reader = new FileReader();
                reader.onload = function (event) {
                    newImageData = event.target.result;
                    var oldImageData = $("#userAvatar").attr("src");

                    if (newImageData === oldImageData) {
                        JSManager.showWarning(L.sameImage);
                        return;
                    }

                    var img = new Image();
                    img.onload = function () {
                        if (img.width < cropSize || img.height < cropSize) {
                            JSManager.showError(L.sizeMsg + ' ' + cropSize + '×' + cropSize + 'px');
                            return;
                        }

                        isChanged = true;

                        $.confirm({
                            title: 'Crop your avatar',
                            columnClass: 'medium',
                            content: '<div class="text-center"><img id="cropImage" src="' + newImageData + '"></div>',
                            buttons: {
                                cancel: {
                                    text: "Cancel",
                                    btnClass: "btn-secondary",
                                    action: function () { if (cropper) cropper.destroy(); }
                                },
                                done: {
                                    text: "Done",
                                    btnClass: "btn-primary",
                                    action: function () {
                                        var canvas = cropper.getCroppedCanvas({ width: cropSize, height: cropSize });
                                        newImageData = canvas.toDataURL("image/png");
                                        $("#userAvatar").attr("src", newImageData);
                                        cropper.destroy();
                                        showSaveButton();
                                    }
                                }
                            },
                            onContentReady: function () {
                                var imageEl = this.$content.find("#cropImage")[0];
                                cropper = new Cropper(imageEl, {
                                    aspectRatio: 1,
                                    viewMode: 1,
                                    autoCropArea: 1,
                                    cropBoxResizable: false,
                                    cropBoxMovable: false,
                                    movable: true,
                                    zoomable: true,
                                    ready: function () {
                                        var container = cropper.getContainerData();
                                        cropper.setCropBoxData({
                                            width: cropSize,
                                            height: cropSize,
                                            left: (container.width - cropSize) / 2,
                                            top: (container.height - cropSize) / 2
                                        });
                                    }
                                });
                            },
                            onClose: function () { if (cropper) cropper.destroy(); }
                        });
                    };
                    img.src = newImageData;
                };
                reader.readAsDataURL(file);
            });

            $("#saveAvatar").off('click.profile').on("click.profile", function () {
                if (!isChanged || !newImageData) return;

                function dataURLtoBlob(dataurl) {
                    var arr = dataurl.split(','), mime = arr[0].match(/:(.*?);/)[1],
                        bstr = atob(arr[1]), n = bstr.length, u8arr = new Uint8Array(n);
                    while (n--) { u8arr[n] = bstr.charCodeAt(n); }
                    return new Blob([u8arr], { type: mime });
                }

                var blob = dataURLtoBlob(newImageData);
                var formData = new FormData();
                formData.append("avatar", blob);

                var withToken = JSManager.addAntiForgeryToken();
                var key = Object.keys(withToken)[0];
                formData.append(key, withToken[key]);

                $.ajax({
                    url: cfg.uploadUrl,
                    method: "POST",
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (response) {
                        if (response.success) {
                            JSManager.showSuccess(L.success);
                            isChanged = false;
                            $("#saveAvatar").hide();
                            $('#userAvatar').attr('src', cfg.avatarUrl + '?v=' + response.version);
                            $('.avatar-img').attr('src', cfg.avatarUrl + '?v=' + response.version);
                        }
                    },
                    error: function () { JSManager.showError('Upload failed'); }
                });
            });
        }
    };
})();
