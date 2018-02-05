stxl.methodOverride = {};
stxl = new Proxy(stxl, {
    get: (target, name, receiver) => {

        if (name in target.__proto__ || typeof target[name] === 'function') { // Method invocation

            if (stxl.methodOverride.hasOwnProperty(name) && typeof stxl.methodOverride[name] === 'function') {

                // Call override function with original method args
                return (...args) => stxl.methodOverride[name](...args);
            }

            // Call original function
            return (...args) => target[name](...args);
        }

        // Instance vars
        return Reflect.get(target, name, receiver);
    }
});

var HotBar = {
    bindSlots: () => {
        stxl.call('hotbarBindSlots', JSON.stringify(HotBar.buildLayoutDescriptors()));
    },
    buildLayoutDescriptors: () => {
        var result = {
            layout: [],
            layoutOrigins: [],
            layoutSizes: []
        };

        for (var i = 0; i < 10; i++) {

            var element = $('#HotBar' + i);
            var rect = element[0].getBoundingClientRect();
            var offset = {};

            offset.top = floatToInt(rect.top) + floatToInt(element.outerHeight() * 0.85);
            offset.left = floatToInt(rect.left) + floatToInt(element.outerWidth() / 2.0);
            result.layout.push(offset);

            var origin = element.offset();
            origin.top = floatToInt(offset.top);
            origin.left = floatToInt(offset.left);
            result.layoutOrigins.push(origin);

            result.layoutSizes.push({
                width: floatToInt($(element).width()),
                height: floatToInt($(element).height())
            });
        }

        return result;
    },
    updateControlHint: (json) => {

        var data = JSON.parse(json);
        var showMain = data.verbMain.length > 0;
        var showAlt = data.verbAlt.length > 0;

        $('#HotBarVerbMain').text(data.verbMain);
        $('#HotBarVerbAlt').text(data.verbAlt);
        $('#HotBarLeftSeparator').text(showMain ? '->' : '');
        $('#HotBarRightSeparator').text(showAlt ? '<-' : '');
        $('#HotBarHintMain').removeClass().addClass(data.hintMain);
        $('#HotBarHintAlt').removeClass().addClass(data.hintAlt);
    }
};

stxl.showHotBar = () => {
    $('#HotBarContainer').fadeIn(250, function () {
        HotBar.bindSlots();
        stxl.call('hotbarAfterShow', '');
    });
};
stxl.hideHotBar = () => {
    $('#HotBarContainer').fadeOut(250, function () {
        stxl.call('hotbarAfterHide', '');
    });
};
stxl.setActiveHotBarItem = function (slotIndex) {
    slotIndex = parseInt(slotIndex);
    if (slotIndex >= 0 && slotIndex <= 10) {

        $('.HotBarCell.Active').removeClass('Active');
        $('#HotBar' + slotIndex).addClass('Active');
    }
};
stxl.rebindHotbar = HotBar.bindSlots;
stxl.methodOverride.updateControlHint = HotBar.updateControlHint;