var formValidation = {
    errors: [],
    valid: function (formName) {
        var $this = this;

        $this.errors = [];
        $('input, textarea, select').removeClass('error');

        $.each($('label.required', $('form[name=' + formName + ']')), function () {
            var field = $(this).closest('div').find('input, textarea, select');
            var type = field.attr('data-type');

            if (field.val() === '') {
                field.addClass('error');
                $this.errors.push(field.attr('name'));
            } else if (type) {
                if (type === 'email' && false === $this.isEmail(field.val())) {
                    field.addClass('error');
                    $this.errors.push(field.attr('name'));
                }
            }
        });

        if (!$this.isValidated()) {
	    //Prevent natural submit
	    return false;
        }
	return true;
    },
    isValidated: function () {
        return this.errors.length === 0;
    },
    isEmail(email) {  
        return /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/.test(email);
    } 
};
