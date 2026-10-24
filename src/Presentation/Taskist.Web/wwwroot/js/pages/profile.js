(function () {
    "use strict";

    var $cfg = $("#profile-config");
    if (!$cfg.length) return;

    var cfg = {
        uploadUrl: $cfg.data("upload-url"),
        removeUrl: $cfg.data("remove-url"),
        updateUrl: $cfg.data("update-url"),
        photoUrl: $cfg.data("photo-url"),
        changePwUrl: $cfg.data("changepw-url"),
        changePwTitle: $cfg.data("changepw-title")
    };

    function refreshPhoto() {
        $("#profilePhoto").attr("src", cfg.photoUrl + "?t=" + Date.now());
    }

    function showErrors(r) {
        if (r && r.errors && r.errors.length) {
            JSManager.showError(r.errors.map(function (e) { return e.message || e.Message; }).filter(Boolean).join(" "));
        } else {
            JSManager.showError(r ? r.message : "Save failed");
        }
    }

    $("#removePhoto").on("click", function () {
        JSManager.ajaxPost(cfg.removeUrl, JSManager.addAntiForgeryToken({}), null, undefined, function (r) {
            if (r && r.status === 2000) { refreshPhoto(); JSManager.showSuccess(r.message); }
            else showErrors(r);
        });
    });

    function uploadBlob(blob) {
        var fd = new FormData();
        fd.append("file", blob, "avatar.jpg");
        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: cfg.uploadUrl,
            type: "POST",
            data: fd,
            processData: false,
            contentType: false,
            headers: token ? { RequestVerificationToken: token } : {},
            success: function (r) {
                if (r && r.status === 2000) {
                    refreshPhoto();
                    JSManager.showSuccess(r.message);
                } else {
                    JSManager.showError(r ? r.message : "Upload failed");
                }
            },
            error: function () { JSManager.showError("Upload failed"); }
        });
    }

    // ---- Crop / zoom modal ----
    var cropper = null;
    var cropModalEl = document.getElementById("photoCropModal");
    var cropModal = cropModalEl ? new bootstrap.Modal(cropModalEl) : null;

    function destroyCropper() {
        if (cropper) { cropper.destroy(); cropper = null; }
    }

    function initCropper() {
        var img = document.getElementById("cropImage");
        destroyCropper();
        cropper = new Cropper(img, {
            aspectRatio: 1,
            viewMode: 1,
            dragMode: "move",
            autoCropArea: 1,
            background: false,
            responsive: true,
            guides: false,
            center: true,
            zoomable: true,
            zoomOnWheel: true,
            minContainerHeight: 320,
            ready: function () { $("#cropZoom").val(0); }
        });
    }

    if (cropModalEl) {
        cropModalEl.addEventListener("hidden.bs.modal", destroyCropper);
        // Init once the modal has finished animating in, so the container has a real size.
        cropModalEl.addEventListener("shown.bs.modal", initCropper);
    }

    function openCropper(dataUrl) {
        document.getElementById("cropImage").src = dataUrl;
        cropModal.show();
    }

    $("#cropZoomIn").on("click", function () { if (cropper) cropper.zoom(0.1); });
    $("#cropZoomOut").on("click", function () { if (cropper) cropper.zoom(-0.1); });
    $("#cropRotate").on("click", function () { if (cropper) cropper.rotate(90); });
    $("#cropZoom").on("input", function () {
        if (!cropper) return;
        // Map the 0..1 slider to a zoom ratio relative to the fitted image.
        var data = cropper.getImageData();
        if (!data || !data.naturalWidth) return;
        var target = (data.width / data.naturalWidth) * (1 + parseFloat(this.value) * 2);
        cropper.zoomTo(target);
    });

    $("#cropSave").on("click", function () {
        if (!cropper) return;
        var canvas = cropper.getCroppedCanvas({
            width: 512,
            height: 512,
            imageSmoothingEnabled: true,
            imageSmoothingQuality: "high"
        });
        if (!canvas) { JSManager.showError("Could not process image"); return; }

        canvas.toBlob(function (blob) {
            if (!blob) { JSManager.showError("Could not process image"); return; }
            cropModal.hide();
            uploadBlob(blob);
        }, "image/jpeg", 0.9);
    });

    $("#profilePhotoInput").on("change", function () {
        var file = this.files && this.files[0];
        var input = this;
        if (!file) return;

        if (file.size > 8 * 1024 * 1024) {
            JSManager.showError("Image is too large (max 8 MB).");
            input.value = "";
            return;
        }

        var reader = new FileReader();
        reader.onload = function (e) {
            openCropper(e.target.result);
            input.value = "";
        };
        reader.readAsDataURL(file);
    });

    $("#btnChangePassword").on("click", function () {
        JSManager.openOffCanvas(cfg.changePwUrl, cfg.changePwTitle);
    });
})();

// Unobtrusive-ajax success handler for the profile details form (data-ajax-success="profileSaved").
function profileSaved(response) {
    var $err = $(".validation-errors").empty();
    if (response.status === 2000) {
        $(".profile-nav__link[data-section='details']").removeClass("has-changes");
        JSManager.showSuccess(response.message);
    } else if (response.status === 2001) {
        var html = "";
        $.each(response.errors || [], function (i, item) {
            var m = item && (item.Message || item.message);
            if (m) html += "<li>" + m + "</li>";
        });
        if (html) {
            $err.html('<div class="alert alert-danger alert-dismissible" role="alert"><ul class="mb-0">' + html + '</ul><button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button></div>');
        } else if (response.message) {
            JSManager.showError(response.message);
        }
    } else {
        JSManager.showError(response.message);
    }
}


/* Git connections (GitHub / GitLab): personal access token or OAuth used to act in the user's own name. */
(function () {
    "use strict";

    function post(url, data, $btn) {
        if ($btn) $btn.prop("disabled", true);
        $.ajax({
            type: "POST",
            url: url,
            data: JSManager.addAntiForgeryToken(data || {}),
            success: function (r) {
                if (r && r.status === 2000) {
                    JSManager.showSuccess(r.message);
                    setTimeout(function () { window.location.hash = "git"; window.location.reload(); }, 700);
                } else {
                    JSManager.showSwal((r && r.message) || "Could not connect the Git account.", "error");
                }
            },
            error: function (xhr) { JSManager.showSwal("Request failed (" + xhr.status + ").", "error"); },
            complete: function () { if ($btn) $btn.prop("disabled", false); }
        });
    }

    function open($btn) {
        if ($btn && $btn.length) JSManager.openForm($btn.data("url"), $btn.data("title"), "col-md-7");
    }

    $(document)
        .on("click", ".js-git-connection", function () { open($(this)); })
        .on("click", ".gitlab-connection__connect", function () {
            var $root = $(this).closest(".gitlab-connection");
            var token = $.trim($root.find(".gitlab-connection__token").val() || "");
            if (!token) { $root.find(".gitlab-connection__token").trigger("focus"); return; }
            post($root.data("connect-url"), { connectionId: $root.data("connection-id"), token: token }, $(this));
        })
        .on("keydown", ".gitlab-connection__token", function (e) {
            if (e.key !== "Enter") return;
            e.preventDefault();
            $(this).closest(".gitlab-connection").find(".gitlab-connection__connect").trigger("click");
        })
        .on("click", ".gitlab-connection__disconnect", function () {
            var $root = $(this).closest(".gitlab-connection");
            var $btn = $(this);
            JSManager.confirm($root.data("disconnect-confirm"), function () {
                post($root.data("disconnect-url"), { connectionId: $root.data("connection-id") }, $btn);
            });
        });

    $(function () {
        var connect = new URLSearchParams(window.location.search).get("connect");
        if (!connect) return;
        if (window.ProfileSections) ProfileSections.show("git");
        var $btn = $(".js-git-connection").filter(function () { return String($(this).data("connection-id")) === connect; });
        open($btn.length ? $btn.first() : $(".js-git-connection").first());
    });
})();


/* Settings-style sections: the left list shows one section at a time, addressed by the URL hash
   (#details, #appearance, #security, #git) so Back works and other pages can deep-link. Links are handled here
   because the layout's <base href> would otherwise resolve "#x" against the site root. Hidden sections keep
   their state; "Personal details" shows a dot while the form has unsaved edits. */
var ProfileSections = (function () {
    "use strict";

    var DEFAULT = "details";

    function known(name) {
        return $(".profile-section[data-section='" + name + "']").length > 0;
    }

    function show(name) {
        if (!known(name)) name = DEFAULT;
        $(".profile-section").each(function () {
            $(this).toggleClass("is-active", $(this).data("section") === name);
        });
        $(".profile-nav__link").each(function () {
            var on = $(this).data("section") === name;
            $(this).toggleClass("is-active", on).attr("aria-current", on ? "page" : null);
            if (on && this.scrollIntoView && window.matchMedia("(max-width: 991.98px)").matches) {
                this.scrollIntoView({ block: "nearest", inline: "nearest" });
            }
        });
        if (window.location.hash.replace("#", "") !== name) {
            history.replaceState(null, "", window.location.pathname + window.location.search + "#" + name);
        }
    }

    function fromHash() {
        show(window.location.hash.replace("#", "") || DEFAULT);
    }

    $(window).on("hashchange popstate", fromHash);

    $(document).on("click", ".profile-nav__link", function (e) {
        e.preventDefault();
        var name = String($(this).data("section"));
        if (window.location.hash.replace("#", "") !== name) {
            history.pushState(null, "", window.location.pathname + window.location.search + "#" + name);
        }
        show(name);
    });

    $(function () {
        fromHash();

        var $form = $(".profile-section[data-section='details'] form");
        $form.on("input change", ":input", function () {
            $(".profile-nav__link[data-section='details']").addClass("has-changes");
        });
    });

    return { show: show };
})();
