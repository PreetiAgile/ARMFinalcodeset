﻿function closeParentFrame() {
    try {
        eval(callParent('closeFrame()', 'function'));
    } catch (ex) {
        //console.log("Error in CloseParentFrame -" + ex.message);
    }
}

function ShowDimmer(status) {
    var dv = $("#waitDiv");

    if (dv.length > 0 && dv != undefined) {
        if (status == true) {
            closeParentFrame();
            $("body").addClass("page-loading");
            document.onkeydown = function EatKeyPress() {
                return false;
            }
        } else {
            $("body").removeClass("page-loading");
            document.onkeydown = function EatKeyPress() {
                return true;
            }
        }
    } else {

        //TODO:Needs to be tested
        if (window.opener != undefined) {

            dv = $("#waitDiv", window.opener.document);
            if (dv.length > 0) {
                if (status == true) {
                    $("body", window.opener.document).addClass("page-loading");
                } else {
                    $("body", window.opener.document).removeClass("page-loading");
                }
            }
        }
    }
}

function loadFrame() {
    parent.$.LoadingOverlay("show");
}

function closeFrame() {
    parent.$.LoadingOverlay("hide", true);
}