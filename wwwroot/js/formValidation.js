var formValidation = {
    errors: [],
    valid: function (formName) {
        var $this = this;

        $this.errors = [];
        $('input, textarea, select').removeClass('error');

        $.each($('label.required', $('form[name=' + formName + ']')), function () {
            var field = $(this).closest('div').find('input, textarea, select');
            if (field.val() === '') {
                field.addClass('error');
                $this.errors.push(field.attr('name'));
            }
        });

        if (this.isValidated()) {
            $('form[name=' + formName + ']').submit();
        }
    },
    isValidated: function () {
        return this.errors.length === 0;
    }
};