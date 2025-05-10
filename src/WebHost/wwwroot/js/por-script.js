// JavaScript for Purchase Order Request Form

document.addEventListener('DOMContentLoaded', function () {
    // Form elements
    const porForm = document.getElementById('porForm');
    const clearFormBtn = document.getElementById('clearForm');

    // Form validation is handled by ASP.NET Core validation scripts

    // Clear form handler
    if (clearFormBtn) {
        clearFormBtn.addEventListener('click', function () {
            // Reset the form
            porForm.reset();

            // Remove any validation messages
            document.querySelectorAll('.text-danger').forEach(el => {
                el.textContent = '';
            });

            // Remove is-invalid classes
            document.querySelectorAll('.is-invalid').forEach(el => {
                el.classList.remove('is-invalid');
            });
        });
    }

    // Add input validation for quantity field
    const quantityInput = document.getElementById('Quantity');
    if (quantityInput) {
        quantityInput.addEventListener('input', function () {
            if (parseInt(this.value) <= 0) {
                this.classList.add('is-invalid');
                const validationSpan = this.nextElementSibling;
                validationSpan.textContent = 'Quantity must be greater than 0';
            } else {
                this.classList.remove('is-invalid');
                const validationSpan = this.nextElementSibling;
                validationSpan.textContent = '';
            }
        });
    }
});