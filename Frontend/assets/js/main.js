// Document Ready Function
document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });
    
    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl)
    });
    
    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(function(alert) {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
});

// API Service Object
const apiService = {
    // Base URL for API - change this to your actual API URL
    baseUrl: 'https://localhost:7182/api',
    
async request(endpoint, options = {}) {
    const url = `${this.baseUrl}${endpoint}`;
    
    // Debug: Check authentication
    const token = localStorage.getItem('authToken');
    console.log('🔐 Auth Token:', token ? 'Present' : 'Missing');
    if (token) {
        console.log('🔐 Token length:', token.length);
        // You can also decode the token to see its contents
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            console.log('🔐 Token payload:', payload);
        } catch (e) {
            console.log('🔐 Could not decode token');
        }
    }
    
    // For FormData, don't set Content-Type header, let the browser set it
    const isFormData = options.body instanceof FormData;
    
    const defaultOptions = {
        headers: isFormData ? {} : {
            'Content-Type': 'application/json'
        }
    };
    
    // Add authorization header if token exists
    if (token) {
        defaultOptions.headers.Authorization = `Bearer ${token}`;
    }
    
    const finalOptions = {
        ...defaultOptions,
        ...options,
        headers: {
            ...defaultOptions.headers,
            ...options.headers
        }
    };
    
    try {
        console.log(`🌐 Making ${finalOptions.method || 'GET'} request to:`, url);
        console.log('🌐 Headers:', finalOptions.headers);
        
        const response = await fetch(url, finalOptions);
        
        // Log response status and headers
        console.log('🌐 Response status:', response.status);
        console.log('🌐 Response headers:', Object.fromEntries(response.headers.entries()));
        
        if (!response.ok) {
            let errorData;
            let responseText;
            
            try {
                responseText = await response.text();
                console.log('🌐 Response text:', responseText);
                errorData = JSON.parse(responseText);
            } catch (e) {
                errorData = {
                    message: `HTTP error! status: ${response.status}`,
                    status: response.status
                };
            }
            
            const error = new Error(errorData.message || `HTTP error! status: ${response.status}`);
            error.status = response.status;
            error.errors = errorData.errors;
            
            throw error;
        }
        
        return await response.json();
    } catch (error) {
        console.error('❌ API request failed:', error);
        throw error;
    }
},
    
    // Authentication methods
    async login(email, password) {
        return this.request('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });
    },
    
    async register(userData) {
        return this.request('/auth/register', {
            method: 'POST',
            body: JSON.stringify(userData)
        });
    },
    
    // User methods
 async getUserProfile() {
    return this.request('/auth/profile'); // Changed from '/users/profile'
},
    
    // Membership methods
 async getUserMembership() {
    return this.request('/memberships');
},
    
    async purchaseMembership(membershipData) {
        return this.request('/memberships', {
            method: 'POST',
            body: JSON.stringify(membershipData)
        });
    },
    
 // Update the getMembershipTypes method in apiService to include inactive types
async getMembershipTypes(includeInactive = true) {
    return this.request(`/membershiptypes?includeInactive=${includeInactive}`);
},

    
   async getMembershipTypeById(id) {
    return this.request(`/membershiptypes/${id}`);
},
    
    // In apiService object

async createMembershipType(membershipData) {
    try {
        console.log('Creating membership type with data:', membershipData);
        const response = await this.request('/membershiptypes', {
            method: 'POST',
            body: JSON.stringify(membershipData)
        });
        console.log('Create membership type response:', response);
        return response;
    } catch (error) {
        console.error('Error in createMembershipType:', error);
        throw error;
    }
},

async updateMembershipType(id, membershipData) {
    try {
        console.log('Updating membership type with data:', membershipData);
        const response = await this.request(`/membershiptypes/${id}`, {
            method: 'PUT',
            body: JSON.stringify(membershipData)
        });
        console.log('Update membership type response:', response);
        return response;
    } catch (error) {
        console.error('Error in updateMembershipType:', error);
        throw error;
    }
},
    
    async deleteMembershipType(id) {
        return this.request(`/membershiptypes/${id}`, {
            method: 'DELETE'
        });
    },
    
   async toggleMembershipTypeStatus(id, isActive) {
    return this.request(`/membershiptypes/${id}/toggle`, {
        method: 'PATCH',
        body: JSON.stringify(isActive)  // Send just the boolean value
    });
},
    
    // Classes methods
    async getClasses() {
        return this.request('/classes');
    },
    
    async getClassById(id) {
        return this.request(`/classes/${id}`);
    },
    
    async createClass(classData) {
        return this.request('/classes', {
            method: 'POST',
            body: JSON.stringify(classData)
        });
    },
    
    async updateClass(id, classData) {
        return this.request(`/classes/${id}`, {
            method: 'PUT',
            body: JSON.stringify(classData)
        });
    },
    
    async deleteClass(id) {
        return this.request(`/classes/${id}`, {
            method: 'DELETE'
        });
    },
    
    async getClassSchedules(classId) {
        return this.request(`/classes/${classId}/schedules`);
    },
    
    async createClassSchedule(scheduleData) {
        return this.request('/classes/schedules', {
            method: 'POST',
            body: JSON.stringify(scheduleData)
        });
    },
    
    async deleteClassSchedule(id) {
        return this.request(`/classes/schedules/${id}`, {
            method: 'DELETE'
        });
    },
    

    // Class Schedule methods
async createClassSchedule(scheduleData) {
    return this.request('/classes/schedules', {
        method: 'POST',
        body: JSON.stringify(scheduleData)
    });
},

// Session Schedule methods
async createSessionSchedule(scheduleData) {
    return this.request('/onlinesessions/schedules', {
        method: 'POST',
        body: JSON.stringify(scheduleData)
    });
},
    // Sessions methods
    async getSessions() {
        return this.request('/onlinesessions');
    },
    
    async getSessionById(id) {
        return this.request(`/onlinesessions/${id}`);
    },
    
    async createSession(sessionData) {
        return this.request('/onlinesessions', {
            method: 'POST',
            body: JSON.stringify(sessionData)
        });
    },
    
    async updateSession(id, sessionData) {
        return this.request(`/onlinesessions/${id}`, {
            method: 'PUT',
            body: JSON.stringify(sessionData)
        });
    },
    
    async deleteSession(id) {
        return this.request(`/onlinesessions/${id}`, {
            method: 'DELETE'
        });
    },
    
    async getSessionSchedules(sessionId) {
        return this.request(`/onlinesessions/${sessionId}/schedules`);
    },
    
    async createSessionSchedule(scheduleData) {
        return this.request('/onlinesessions/schedules', {
            method: 'POST',
            body: JSON.stringify(scheduleData)
        });
    },
    
    async deleteSessionSchedule(id) {
        return this.request(`/onlinesessions/schedules/${id}`, {
            method: 'DELETE'
        });
    },
    
    // Trainers methods
    async getTrainers() {
        return this.request('/trainers');
    },
    
    async getTrainerById(id) {
        return this.request(`/trainers/${id}`);
    },
    
  // In apiService object, update createTrainer and updateTrainer methods

async createTrainer(trainerData, imageFile = null) {
    const formData = new FormData();
    
    // Append all trainer data with correct casing
    formData.append('Name', trainerData.name);
    formData.append('Specialization', trainerData.specialization);
    formData.append('ExperienceYears', trainerData.experienceYears);
    formData.append('Certifications', trainerData.certifications || '');
    formData.append('Bio', trainerData.bio);
    
    // Add image file if provided
    if (imageFile) {
        formData.append('ImageFile', imageFile);
    }
    
    return this.request('/trainers', {
        method: 'POST',
        body: formData,
        headers: {} // Let browser set Content-Type for multipart/form-data
    });
},

// Update updateTrainer method
async updateTrainer(id, trainerData, imageFile = null) {
    const formData = new FormData();
    
    // Append all trainer data with correct casing
    formData.append('Name', trainerData.name);
    formData.append('Specialization', trainerData.specialization);
    formData.append('ExperienceYears', trainerData.experienceYears);
    formData.append('Certifications', trainerData.certifications || '');
    formData.append('Bio', trainerData.bio);
    
    // Add image file if provided
    if (imageFile) {
        formData.append('ImageFile', imageFile);
    }
    
    return this.request(`/trainers/${id}`, {
        method: 'PUT',
        body: formData,
        headers: {} // Let browser set Content-Type for multipart/form-data
    });
},


    
    async deleteTrainer(id) {
        return this.request(`/trainers/${id}`, {
            method: 'DELETE'
        });
    },
    
    async getTrainerClasses(trainerId) {
        return this.request(`/trainers/${trainerId}/classes`);
    },
    
    async getTrainerSessions(trainerId) {
        return this.request(`/trainers/${trainerId}/sessions`);
    },
    
    // Booking methods
    async createBooking(bookingData) {
        return this.request('/bookings', {
            method: 'POST',
            body: JSON.stringify(bookingData)
        });
    },
    
    async getBookingDetails(bookingId) {
        return this.request(`/bookings/${bookingId}`);
    },
    
   async getUserBookings() {
    return this.request('/bookings');
},
    
    async cancelBooking(bookingId) {
        return this.request(`/bookings/${bookingId}`, {
            method: 'DELETE'
        });
    },
    
    // Products methods
    async getProducts() {
        return this.request('/products');
    },
    
    async getProductById(id) {
        return this.request(`/products/${id}`);
    },
    
    async getProductsByCategory(category) {
        return this.request(`/products/category/${category}`);
    },
    
 async createProduct(productData, imageFile = null) {
    const formData = new FormData();
    
    // Append all product data with correct casing
    formData.append('Name', productData.name);
    formData.append('Description', productData.description);
    formData.append('Category', productData.category);
    formData.append('Price', productData.price);
    formData.append('StockQuantity', productData.stockQuantity);
    
    // Add image file if provided
    if (imageFile) {
        formData.append('ImageFile', imageFile);
    }
    
    return this.request('/products', {
        method: 'POST',
        body: formData,
        headers: {} // Let browser set Content-Type for multipart/form-data
    });
},

// Update updateProduct method
// In apiService object
async updateProduct(id, productData, imageFile = null, existingImageUrl = null) {
    const formData = new FormData();
    
    // Append all product data
    formData.append('Name', productData.name);
    formData.append('Description', productData.description);
    formData.append('Category', productData.category);
    formData.append('Price', productData.price);
    formData.append('StockQuantity', productData.stockQuantity);
    
    // Add existing image URL for reference
    if (existingImageUrl) {
        formData.append('ExistingImageUrl', existingImageUrl);
    }
    
    // Add image file if provided
    if (imageFile) {
        formData.append('ImageFile', imageFile);
    }
    
    return this.request(`/products/${id}`, {
        method: 'PUT',
        body: formData,
        headers: {} // Let browser set Content-Type for multipart/form-data
    });
},
    
    async deleteProduct(id) {
        return this.request(`/products/${id}`, {
            method: 'DELETE'
        });
    },
    
  async updateProductStock(id, quantity) {
    return this.request(`/products/${id}/stock`, {
        method: 'PATCH',
        body: JSON.stringify(quantity)  // Send just the integer value, not an object
    });
},
    
    // Order methods
    async createOrder(orderData) {
        return this.request('/orders', {
            method: 'POST',
            body: JSON.stringify(orderData)
        });
    },
    
// Fix the getUserOrders method in apiService
async getUserOrders() {
    return this.request('/orders/user'); // Changed to user-specific endpoint
},
    
    async getOrderById(id) {
        return this.request(`/orders/${id}`);
    },
    
    async updateOrderStatus(id, status) {
        return this.request(`/orders/${id}/status`, {
            method: 'PATCH',
            body: JSON.stringify({ status })
        });
    },
    
    async cancelOrder(id) {
        return this.request(`/orders/${id}`, {
            method: 'DELETE'
        });
    },
    
    // Promotions methods
    async getPromotions() {
        return this.request('/promotions');
    },
    
    async getPromotionById(id) {
        return this.request(`/promotions/${id}`);
    },
    
    async getPromotionByCode(code) {
        return this.request(`/promotions/code/${code}`);
    },
    
    async createPromotion(promotionData) {
        return this.request('/promotions', {
            method: 'POST',
            body: JSON.stringify(promotionData)
        });
    },
    
    async updatePromotion(id, promotionData) {
        return this.request(`/promotions/${id}`, {
            method: 'PUT',
            body: JSON.stringify(promotionData)
        });
    },
    
    async deletePromotion(id) {
        return this.request(`/promotions/${id}`, {
            method: 'DELETE'
        });
    },
    
    async validatePromotion(code, orderAmount) {
        return this.request('/promotions/validate', {
            method: 'POST',
            body: JSON.stringify({ code, orderAmount })
        });
    },
    async calculateDiscount(code, orderAmount) {
    return this.request('/orders/apply-promotion', {
        method: 'POST',
        body: JSON.stringify({ code, orderAmount })
    });
},
    
    // Admin methods
    async getDashboardStats() {
        return this.request('/admin/dashboard/stats');
    },
    
    async getAllUsers(page = 1, pageSize = 10) {
        return this.request(`/admin/users?page=${page}&pageSize=${pageSize}`);
    },
    
    async getUserDetails(userId) {
        return this.request(`/admin/users/${userId}`);
    },
    
    async getAllBookings(startDate, endDate) {
        const params = new URLSearchParams();
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);
        
        return this.request(`/admin/bookings?${params.toString()}`);
    },
    
    async getRevenueReport(startDate, endDate) {
        return this.request(`/admin/revenue?startDate=${startDate}&endDate=${endDate}`);
    },
    
    async getAllOrders() {
        return this.request('/admin/orders');
    }
};

// Auth Service Object
const authService = {
    // Check if user is logged in
    isLoggedIn() {
        return !!localStorage.getItem('authToken');
    },
    
    // Get current user
    getCurrentUser() {
        const userJson = localStorage.getItem('currentUser');
        return userJson ? JSON.parse(userJson) : null;
    },
    
    // Check if user is admin
    isAdmin() {
        const token = localStorage.getItem('authToken');
        if (!token) return false;
        
        try {
            const parts = token.split('.');
            if (parts.length !== 3) return false;
            
            const payloadBase64Url = parts[1];
            let payloadBase64 = payloadBase64Url.replace(/-/g, '+').replace(/_/g, '/');
            
            while (payloadBase64.length % 4) {
                payloadBase64 += '=';
            }
            
            const payload = JSON.parse(atob(payloadBase64));
            
            const roleClaim1 = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
            const roleClaim2 = payload['role'];
            const roleClaim3 = payload['Role'];
            
            return roleClaim1 === 'Admin' || roleClaim2 === 'Admin' || roleClaim3 === 'Admin';
        } catch (e) {
            console.error('Error parsing JWT token:', e);
            return false;
        }
    },
    
    // Logout user
    logout() {
        localStorage.removeItem('authToken');
        localStorage.removeItem('currentUser');
        window.location.href = '../login.html';
    },
    
    // Save auth data
    saveAuthData(token, user) {
        localStorage.setItem('authToken', token);
        localStorage.setItem('currentUser', JSON.stringify(user));
    },

     // NEW: Update user profile
    async updateUserProfile(profileData) {
        try {
            const response = await apiService.request('/auth/profile', {
                method: 'PUT',
                body: JSON.stringify(profileData)
            });
            
            // Update local user data
            const currentUser = this.getCurrentUser();
            if (currentUser) {
                if (profileData.name) currentUser.name = profileData.name;
                if (profileData.phone) currentUser.phone = profileData.phone;
                localStorage.setItem('currentUser', JSON.stringify(currentUser));
            }
            
            return response;
        } catch (error) {
            console.error('Error updating profile:', error);
            throw error;
        }
    },
    
    // NEW: Change password
    async changePassword(currentPassword, newPassword) {
        try {
            const response = await apiService.request('/auth/change-password', {
                method: 'POST',
                body: JSON.stringify({
                    currentPassword,
                    newPassword
                })
            });
            
            return response;
        } catch (error) {
            console.error('Error changing password:', error);
            throw error;
        }
    }
};

// UI Helper Functions
const uiHelper = {
    // Show alert message
    showAlert(message, type = 'info') {
        const alertContainer = document.getElementById('alert-container') || this.createAlertContainer();
        
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        
        alertContainer.appendChild(alert);
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    },
    
    // Create alert container if it doesn't exist
    createAlertContainer() {
        const container = document.createElement('div');
        container.id = 'alert-container';
        container.className = 'container mt-3';
        document.querySelector('main').prepend(container);
        return container;
    },
    
    // Show loading spinner
    showLoading(element) {
        const spinner = document.createElement('div');
        spinner.className = 'spinner-container';
        spinner.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>';
        
        if (element) {
            element.innerHTML = '';
            element.appendChild(spinner);
        } else {
            document.body.appendChild(spinner);
        }
        
        return spinner;
    },
    
    // Hide loading spinner
    hideLoading(spinner) {
        if (spinner && spinner.parentNode) {
            spinner.parentNode.removeChild(spinner);
        }
    },
    
    // Format date
    formatDate(dateString) {
        const options = { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' };
        return new Date(dateString).toLocaleDateString(undefined, options);
    },
    
    // Format currency
    formatCurrency(amount) {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(amount);
    },
    
    // Render error message
    renderError(element, message) {
        element.innerHTML = `<div class="alert alert-danger">${message}</div>`;
    },
    
    // Confirm dialog
    confirm(message, callback) {
        if (confirm(message)) {
            callback();
        }
    },
    
    // Show toast notification
    showToast(message, type = 'info') {
        // Create toast container if it doesn't exist
        let toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toast-container';
            toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            document.body.appendChild(toastContainer);
        }
        
        // Create toast element
        const toastEl = document.createElement('div');
        toastEl.className = `toast align-items-center text-white bg-${type} border-0`;
        toastEl.setAttribute('role', 'alert');
        toastEl.setAttribute('aria-live', 'assertive');
        toastEl.setAttribute('aria-atomic', 'true');
        
        toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;
        
        toastContainer.appendChild(toastEl);
        
        const toast = new bootstrap.Toast(toastEl, {
            autohide: true,
            delay: 5000
        });
        
        toast.show();
    }
};

// Form Validation Helper
const formValidator = {
    // Validate email
    isValidEmail(email) {
        const re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(String(email).toLowerCase());
    },
    
    // Validate password (at least 6 characters)
    isValidPassword(password) {
        return password.length >= 6;
    },
    
    // Validate required fields
    validateRequired(form) {
        const requiredFields = form.querySelectorAll('[required]');
        let isValid = true;
        
        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                field.classList.add('is-invalid');
                isValid = false;
            } else {
                field.classList.remove('is-invalid');
            }
        });
        
        return isValid;
    },
    
    // Validate form
    validateForm(form, rules) {
        let isValid = true;
        
        // First validate required fields
        if (!this.validateRequired(form)) {
            isValid = false;
        }
        
        // Then validate specific rules
        for (const [fieldName, rule] of Object.entries(rules)) {
            const field = form.querySelector(`[name="${fieldName}"]`);
            if (!field) continue;
            
            const value = field.value.trim();
            
            if (rule.email && !this.isValidEmail(value)) {
                this.setFieldError(field, 'Please enter a valid email address');
                isValid = false;
            } else if (rule.minLength && value.length < rule.minLength) {
                this.setFieldError(field, `Minimum length is ${rule.minLength} characters`);
                isValid = false;
            } else if (rule.match && value !== form.querySelector(`[name="${rule.match}"]`).value) {
                this.setFieldError(field, 'Fields do not match');
                isValid = false;
            } else {
                this.clearFieldError(field);
            }
        }
        
        return isValid;
    },
    
    // Set field error
    setFieldError(field, message) {
        field.classList.add('is-invalid');
        
        let feedback = field.nextElementSibling;
        if (!feedback || !feedback.classList.contains('invalid-feedback')) {
            feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            field.parentNode.insertBefore(feedback, field.nextSibling);
        }
        
        feedback.textContent = message;
    },
    
    // Clear field error
    clearFieldError(field) {
        field.classList.remove('is-invalid');
        const feedback = field.nextElementSibling;
        if (feedback && feedback.classList.contains('invalid-feedback')) {
            feedback.textContent = '';
        }
    }
};

// Utility Functions
const utils = {
    // Get query parameter from URL
    getQueryParam(param) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(param);
    },
    
    // Format date for input fields (YYYY-MM-DD)
    formatDateForInput(date) {
        const d = new Date(date);
        let month = (d.getMonth() + 1).toString();
        let day = d.getDate().toString();
        const year = d.getFullYear();
        
        if (month.length < 2) month = '0' + month;
        if (day.length < 2) day = '0' + day;
        
        return [year, month, day].join('-');
    },
    
    // Calculate days between two dates
    daysBetween(date1, date2) {
        const oneDay = 24 * 60 * 60 * 1000; // hours*minutes*seconds*milliseconds
        const diffDays = Math.round(Math.abs((date1 - date2) / oneDay));
        return diffDays;
    },
    
    // Debounce function to limit how often a function can be called
    debounce(func, wait, immediate) {
        let timeout;
        return function() {
            const context = this, args = arguments;
            const later = function() {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    },
    
    // Generate a unique ID
    generateUniqueId() {
        return Date.now().toString(36) + Math.random().toString(36).substr(2);
    }
};