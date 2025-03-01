// Global Variables
let currentUser = null;
let authToken = localStorage.getItem('token') || localStorage.getItem('authToken');

// API Configuration
const API_BASE_URL = window.location.origin;

// Error Handling and Retry Configuration
const ERROR_CONFIG = {
    maxRetries: 3,
    retryDelay: 1000, // milliseconds
    timeoutDuration: 10000, // 10 seconds
};

// Enhanced Error Handling Utilities
class ApiErrorHandler {
    static handleApiError(error, operation = 'operation') {
        console.error(`${operation} failed:`, error);
        
        if (error.name === 'AbortError') {
            showNotification('Operation timed out. Please try again.', 'error');
            return;
        }
        
        if (!navigator.onLine) {
            showNotification('No internet connection. Please check your network.', 'error');
            return;
        }
        
        if (error.status) {
            switch (error.status) {
                case 400:
                    showNotification('Invalid request. Please check your input.', 'error');
                    break;
                case 401:
                    showNotification('Session expired. Please log in again.', 'error');
                    this.handleAuthError();
                    break;
                case 403:
                    showNotification('Access denied. Insufficient permissions.', 'error');
                    break;
                case 404:
                    showNotification('Resource not found.', 'error');
                    break;
                case 429:
                    showNotification('Too many requests. Please wait and try again.', 'error');
                    break;
                case 500:
                    showNotification('Server error. Please try again later.', 'error');
                    break;
                case 503:
                    showNotification('Service temporarily unavailable.', 'error');
                    break;
                default:
                    showNotification(`${operation} failed. Please try again.`, 'error');
            }
        } else {
            showNotification(`${operation} failed. Please try again.`, 'error');
        }
    }
    
    static handleAuthError() {
        // Clear stored auth data
        localStorage.removeItem('authToken');
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        authToken = null;
        currentUser = null;
        
        // Update UI
        updateAuthUI();
        
        // Redirect to login if needed
        if (window.location.pathname.includes('admin')) {
            window.location.href = '/';
        }
    }
    
    static async retryOperation(operation, maxRetries = ERROR_CONFIG.maxRetries) {
        for (let attempt = 1; attempt <= maxRetries; attempt++) {
            try {
                return await operation();
            } catch (error) {
                if (attempt === maxRetries) {
                    throw error;
                }
                
                console.warn(`Attempt ${attempt} failed, retrying...`);
                await new Promise(resolve => setTimeout(resolve, ERROR_CONFIG.retryDelay * attempt));
            }
        }
    }
}

// Enhanced Fetch with Timeout and Error Handling
async function fetchWithTimeout(url, options = {}) {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), ERROR_CONFIG.timeoutDuration);
    
    try {
        const response = await fetch(url, {
            ...options,
            signal: controller.signal,
            headers: {
                'Content-Type': 'application/json',
                ...(authToken && { 'Authorization': `Bearer ${authToken}` }),
                ...options.headers
            }
        });
        
        clearTimeout(timeout);
        
        if (!response.ok) {
            const error = new Error(`HTTP ${response.status}`);
            error.status = response.status;
            error.response = response;
            throw error;
        }
        
        return response;
    } catch (error) {
        clearTimeout(timeout);
        throw error;
    }
}

// Network Status Monitor
function setupNetworkMonitoring() {
    window.addEventListener('online', () => {
        showNotification('Connection restored', 'success');
    });
    
    window.addEventListener('offline', () => {
        showNotification('Connection lost. Please check your network.', 'error');
    });
}

// Global Error Handler
window.addEventListener('error', (event) => {
    console.error('Global error:', event.error);
    showNotification('An unexpected error occurred. Please refresh the page.', 'error');
});

window.addEventListener('unhandledrejection', (event) => {
    console.error('Unhandled promise rejection:', event.reason);
    showNotification('An unexpected error occurred. Please try again.', 'error');
    event.preventDefault();
});

// Initialize the application
document.addEventListener('DOMContentLoaded', function() {
    // Setup network monitoring and error handling
    setupNetworkMonitoring();
    
    initializeApp();
    setupEventListeners();
    loadFeaturedContent();
});

// Initialize Application
function initializeApp() {
    // Check if user is logged in
    if (authToken) {
        checkAuthStatus();
    }
    
    // Setup navbar scroll effect
    setupNavbarScroll();
    
    // Setup search tabs
    setupSearchTabs();
    
    // Setup mobile menu
    setupMobileMenu();
    
    // Hide loading spinner
    hideLoading();
}

// Setup Event Listeners
function setupEventListeners() {
    // Search form handlers
    document.getElementById('packages-search')?.addEventListener('submit', function(e) {
        e.preventDefault();
        searchPackages();
    });
    
    document.getElementById('hotels-search')?.addEventListener('submit', function(e) {
        e.preventDefault();
        searchHotels();
    });
    
    // Modal handlers
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('modal')) {
            closeModal(e.target.id);
        }
    });
    
    // Smooth scrolling for navigation links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
}

// Navbar Scroll Effect
function setupNavbarScroll() {
    window.addEventListener('scroll', function() {
        const navbar = document.querySelector('.navbar');
        if (window.scrollY > 100) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    });
}

// Search Tabs
function setupSearchTabs() {
    const tabButtons = document.querySelectorAll('.tab-btn');
    const searchForms = document.querySelectorAll('.search-form');
    
    tabButtons.forEach(button => {
        button.addEventListener('click', function() {
            const tabId = this.getAttribute('data-tab');
            
            // Update active tab
            tabButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            
            // Show corresponding form
            searchForms.forEach(form => form.classList.add('hidden'));
            document.getElementById(tabId + '-search')?.classList.remove('hidden');
        });
    });
}

// Mobile Menu
function setupMobileMenu() {
    const hamburger = document.getElementById('hamburger');
    const navMenu = document.getElementById('nav-menu');
    
    hamburger?.addEventListener('click', function() {
        navMenu.classList.toggle('active');
    });
    
    // Close menu when clicking on links
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', function() {
            navMenu.classList.remove('active');
        });
    });
}

// Authentication Functions
function showLogin() {
    closeModal('register-modal');
    document.getElementById('login-modal').classList.add('active');
}

function showRegister() {
    closeModal('login-modal');
    document.getElementById('register-modal').classList.add('active');
}

function closeModal(modalId) {
    document.getElementById(modalId)?.classList.remove('active');
}

async function handleLogin(event) {
    event.preventDefault();
    showLoading();
    
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;
    
    try {
        const response = await ApiErrorHandler.retryOperation(async () => {
            return await fetchWithTimeout(`${API_BASE_URL}/api/auth/login`, {
                method: 'POST',
                body: JSON.stringify({ email, password })
            });
        });
        
        const data = await response.json();
        
        authToken = data.token;
        localStorage.setItem('token', authToken); // Use consistent token key
        localStorage.setItem('authToken', authToken); // Keep for backwards compatibility
        currentUser = data.user;
        localStorage.setItem('user', JSON.stringify(currentUser)); // Store user data
        
        closeModal('login-modal');
        updateUIForLoggedInUser();
        
        // Check if user is admin and redirect
        if (currentUser.roles && currentUser.roles.includes('Admin')) {
            showNotification('Welcome Admin! Redirecting to admin panel...', 'success');
            setTimeout(() => {
                window.location.href = '/admin.html';
            }, 1500);
        } else {
            showNotification('Login successful!', 'success');
        }
        
    } catch (error) {
        ApiErrorHandler.handleApiError(error, 'Login');
    } finally {
        hideLoading();
    }
}

// Global variable to store registration data temporarily
let pendingRegistration = null;

async function handleRegister(event) {
    event.preventDefault();
    showLoading();
    
    const firstName = document.getElementById('first-name').value;
    const lastName = document.getElementById('last-name').value;
    const email = document.getElementById('register-email').value;
    const password = document.getElementById('register-password').value;
    const confirmPassword = document.getElementById('confirm-password').value;
    const dateOfBirth = document.getElementById('date-of-birth').value;
    
    if (password !== confirmPassword) {
        showNotification('Passwords do not match', 'error');
        hideLoading();
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                firstName,
                lastName,
                email,
                password,
                confirmPassword,
                dateOfBirth
            })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            if (data.requiresOtp) {
                // OTP flow (if implemented later)
                pendingRegistration = {
                    firstName,
                    lastName,
                    email,
                    password,
                    dateOfBirth
                };
                
                closeModal('register-modal');
                showOtpModal(email);
                showNotification('Verification code sent to your email!', 'success');
            } else if (data.token) {
                // Immediate login flow
                localStorage.setItem('token', data.token);
                localStorage.setItem('user', JSON.stringify(data.user));
                currentUser = data.user;
                
                closeModal('register-modal');
                showNotification(`Welcome ${data.user.firstName}! Registration successful.`, 'success');
                updateAuthUI();
                
                // Check if user is admin and redirect
                if (currentUser.roles && currentUser.roles.includes('Admin')) {
                    showNotification('Welcome Admin! Redirecting to admin panel...', 'success');
                    setTimeout(() => {
                        window.location.href = '/admin.html';
                    }, 1000);
                    return;
                }
            } else {
                showNotification(data.message || 'Registration successful', 'success');
            }
        } else {
            showNotification(data.message || 'Registration failed', 'error');
        }
    } catch (error) {
        console.error('Registration error:', error);
        showNotification('Registration failed. Please try again.', 'error');
    } finally {
        hideLoading();
    }
}

function showOtpModal(email) {
    document.getElementById('otp-email-display').textContent = email;
    document.getElementById('otp-code').value = '';
    document.getElementById('otp-modal').classList.add('active');
}

async function handleOtpVerification(event) {
    event.preventDefault();
    showLoading();
    
    if (!pendingRegistration) {
        showNotification('Registration data not found. Please try again.', 'error');
        hideLoading();
        return;
    }
    
    const otp = document.getElementById('otp-code').value;
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/auth/verify-otp`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email: pendingRegistration.email,
                otp: otp,
                firstName: pendingRegistration.firstName,
                lastName: pendingRegistration.lastName,
                password: pendingRegistration.password,
                dateOfBirth: pendingRegistration.dateOfBirth
            })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            // Registration successful
            authToken = data.token;
            localStorage.setItem('authToken', authToken);
            currentUser = data.user;
            
            closeModal('otp-modal');
            updateUIForLoggedInUser();
            pendingRegistration = null;
            
            // Check if user is admin and redirect
            if (currentUser.roles && currentUser.roles.includes('Admin')) {
                showNotification('Welcome Admin! Redirecting to admin panel...', 'success');
                setTimeout(() => {
                    window.location.href = '/admin.html';
                }, 2000);
            } else {
                showNotification('Registration completed successfully! Welcome to TraWell!', 'success');
            }
        } else {
            showNotification(data.message || 'Invalid verification code', 'error');
        }
    } catch (error) {
        console.error('OTP verification error:', error);
        showNotification('Verification failed. Please try again.', 'error');
    } finally {
        hideLoading();
    }
}

async function resendOtp() {
    if (!pendingRegistration) {
        showNotification('No pending registration found', 'error');
        return;
    }
    
    showLoading();
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/auth/resend-otp`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email: pendingRegistration.email
            })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            showNotification('New verification code sent!', 'success');
        } else {
            showNotification(data.message || 'Failed to resend code', 'error');
        }
    } catch (error) {
        console.error('Resend OTP error:', error);
        showNotification('Failed to resend code. Please try again.', 'error');
    } finally {
        hideLoading();
    }
}

async function checkAuthStatus() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/auth/profile`, {
            headers: {
                'Authorization': `Bearer ${authToken}`
            }
        });
        
        if (response.ok) {
            currentUser = await response.json();
            updateUIForLoggedInUser();
        } else {
            logout();
        }
    } catch (error) {
        console.error('Auth check error:', error);
        logout();
    }
}

function updateUIForLoggedInUser() {
    const navAuth = document.querySelector('.nav-auth');
    if (navAuth && currentUser) {
        navAuth.innerHTML = `
            <div class="user-menu">
                <span>Hello, ${currentUser.firstName}</span>
                <button class="btn-outline" onclick="logout()">Logout</button>
            </div>
        `;
    }
}

function updateAuthUI() {
    const navAuth = document.querySelector('.nav-auth');
    if (!navAuth) return;
    
    if (currentUser && authToken) {
        // User is logged in
        navAuth.innerHTML = `
            <div class="user-menu">
                <span>Hello, ${currentUser.firstName}</span>
                <button class="btn-outline" onclick="logout()">Logout</button>
            </div>
        `;
    } else {
        // User is not logged in
        navAuth.innerHTML = `
            <button class="btn-outline" onclick="showLogin()">Login</button>
            <button class="btn-primary" onclick="showRegister()">Sign Up</button>
        `;
    }
}

function logout() {
    authToken = null;
    currentUser = null;
    localStorage.removeItem('authToken');
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    
    const navAuth = document.querySelector('.nav-auth');
    if (navAuth) {
        navAuth.innerHTML = `
            <button class="btn-outline" onclick="showLogin()">Login</button>
            <button class="btn-primary" onclick="showRegister()">Sign Up</button>
        `;
    }
    
    showNotification('Logged out successfully', 'success');
}

// Search Functions
async function searchPackages() {
    showLoading();
    
    const destination = document.getElementById('destination').value;
    const startDate = document.getElementById('start-date').value;
    const guests = document.getElementById('guests').value;
    
    try {
        const params = new URLSearchParams();
        if (destination) params.append('destination', destination);
        if (startDate) params.append('startDate', startDate);
        if (guests) params.append('guests', guests);
        
        // Temporarily use sample data since API endpoint doesn't exist
        displaySamplePackages();
        document.getElementById('packages').scrollIntoView({ behavior: 'smooth' });
    } catch (error) {
        console.error('Search error:', error);
        showNotification('Search failed. Please try again.', 'error');
    } finally {
        hideLoading();
    }
}

async function searchHotels() {
    showLoading();
    
    const destination = document.getElementById('hotel-destination').value;
    const checkIn = document.getElementById('check-in').value;
    const checkOut = document.getElementById('check-out').value;
    const rooms = document.getElementById('rooms').value;
    
    try {
        const params = new URLSearchParams();
        if (destination) params.append('destination', destination);
        if (checkIn) params.append('checkIn', checkIn);
        if (checkOut) params.append('checkOut', checkOut);
        if (rooms) params.append('rooms', rooms);
        
        // Temporarily use sample data since API endpoint doesn't exist
        displaySampleHotels();
        document.getElementById('hotels').scrollIntoView({ behavior: 'smooth' });
    } catch (error) {
        console.error('Search error:', error);
        showNotification('Search failed. Please try again.', 'error');
    } finally {
        hideLoading();
    }
}

// Content Loading Functions
async function loadFeaturedContent() {
    try {
        await Promise.all([
            loadFeaturedPackages(),
            loadFeaturedHotels(),
            loadPopularDestinations()
        ]);
    } catch (error) {
        console.error('Error loading featured content:', error);
    }
}

async function loadFeaturedPackages() {
    try {
        console.log('Loading featured packages...');
        // For now, display sample packages since API endpoint doesn't exist
        displaySamplePackages();
    } catch (error) {
        console.error('Error loading featured packages:', error);
        displaySamplePackages();
    }
}

async function loadFeaturedHotels() {
    try {
        console.log('Loading featured hotels...');
        // For now, display sample hotels since API endpoint doesn't exist
        displaySampleHotels();
    } catch (error) {
        console.error('Error loading featured hotels:', error);
        displaySampleHotels();
    }
}

async function loadPopularDestinations() {
    try {
        console.log('Loading popular destinations...');
        // For now, display sample destinations since API endpoint doesn't exist
        displaySampleDestinations();
    } catch (error) {
        console.error('Error loading destinations:', error);
        displaySampleDestinations();
    }
}

// Display Functions
function displayPackages(packages, containerId = 'featured-packages') {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    if (!packages || packages.length === 0) {
        container.innerHTML = '<p class="text-center">No packages found.</p>';
        return;
    }
    
    container.innerHTML = packages.map(pkg => `
        <div class="package-card">
            <div class="card-image">
                <img src="${pkg.imageUrl || 'https://images.unsplash.com/photo-1469474968028-56623f02e42e?w=400'}" 
                     alt="${pkg.title}" onerror="this.src='https://images.unsplash.com/photo-1469474968028-56623f02e42e?w=400'">
            </div>
            <div class="card-content">
                <h3 class="card-title">${pkg.title}</h3>
                <p class="card-description">${pkg.description}</p>
                <div class="card-price">$${pkg.price}</div>
                <div class="card-rating">
                    <div class="stars">${generateStars(pkg.averageRating || 4.5)}</div>
                    <span class="rating-text">(${pkg.reviewCount || 0} reviews)</span>
                </div>
                <div class="card-features">
                    <span class="feature-tag">${pkg.duration} days</span>
                    <span class="feature-tag">${pkg.destination}</span>
                </div>
                <button class="btn-primary full-width" onclick="viewPackageDetails(${pkg.id})">
                    View Details
                </button>
            </div>
        </div>
    `).join('');
}

function displayHotels(hotels, containerId = 'featured-hotels') {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    if (!hotels || hotels.length === 0) {
        container.innerHTML = '<p class="text-center">No hotels found.</p>';
        return;
    }
    
    container.innerHTML = hotels.map(hotel => `
        <div class="hotel-card">
            <div class="card-image">
                <img src="${hotel.imageUrl || 'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=400'}" 
                     alt="${hotel.name}" onerror="this.src='https://images.unsplash.com/photo-1566073771259-6a8506099945?w=400'">
            </div>
            <div class="card-content">
                <h3 class="card-title">${hotel.name}</h3>
                <p class="card-description">${hotel.description}</p>
                <div class="card-price">$${hotel.pricePerNight}/night</div>
                <div class="card-rating">
                    <div class="stars">${generateStars(hotel.averageRating || 4.5)}</div>
                    <span class="rating-text">(${hotel.reviewCount || 0} reviews)</span>
                </div>
                <div class="card-features">
                    <span class="feature-tag">${hotel.starRating} Star</span>
                    <span class="feature-tag">${hotel.location}</span>
                </div>
                <button class="btn-primary full-width" onclick="viewHotelDetails(${hotel.id})">
                    View Details
                </button>
            </div>
        </div>
    `).join('');
}

function displayDestinations(destinations, containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    if (!destinations || destinations.length === 0) {
        displaySampleDestinations();
        return;
    }
    
    container.innerHTML = destinations.map(dest => `
        <div class="destination-card">
            <div class="card-image">
                <img src="${dest.imageUrl || 'https://images.unsplash.com/photo-1503220317375-aaad61436b1b?w=400'}" 
                     alt="${dest.name}" onerror="this.src='https://images.unsplash.com/photo-1503220317375-aaad61436b1b?w=400'">
            </div>
            <div class="card-content">
                <h3 class="card-title">${dest.name}</h3>
                <p class="card-description">${dest.packageCount} packages available</p>
                <button class="btn-outline full-width" onclick="searchDestination('${dest.name}')">
                    Explore Packages
                </button>
            </div>
        </div>
    `).join('');
}

// Sample Data Functions (fallbacks)
function displaySamplePackages() {
    const samplePackages = [
        {
            id: 1,
            title: "Bali Paradise Adventure",
            description: "Explore the beautiful beaches and temples of Bali",
            price: 899,
            duration: 7,
            destination: "Bali, Indonesia",
            averageRating: 4.8,
            reviewCount: 124,
            imageUrl: "https://images.unsplash.com/photo-1518548419970-58e3b4079ab2?w=400"
        },
        {
            id: 2,
            title: "Swiss Alps Experience",
            description: "Breathtaking mountain views and alpine adventures",
            price: 1299,
            duration: 10,
            destination: "Switzerland",
            averageRating: 4.9,
            reviewCount: 89,
            imageUrl: "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400"
        },
        {
            id: 3,
            title: "Tokyo Cultural Journey",
            description: "Immerse yourself in Japanese culture and cuisine",
            price: 1099,
            duration: 8,
            destination: "Tokyo, Japan",
            averageRating: 4.7,
            reviewCount: 156,
            imageUrl: "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=400"
        }
    ];
    
    displayPackages(samplePackages);
}

function displaySampleHotels() {
    const sampleHotels = [
        {
            id: 1,
            name: "Grand Paradise Resort",
            description: "Luxury beachfront resort with world-class amenities",
            pricePerNight: 299,
            location: "Maldives",
            starRating: 5,
            averageRating: 4.9,
            reviewCount: 234,
            imageUrl: "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=400"
        },
        {
            id: 2,
            name: "Urban Boutique Hotel",
            description: "Modern hotel in the heart of the city",
            pricePerNight: 189,
            location: "New York",
            starRating: 4,
            averageRating: 4.6,
            reviewCount: 178,
            imageUrl: "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=400"
        },
        {
            id: 3,
            name: "Mountain Lodge Retreat",
            description: "Cozy lodge with stunning mountain views",
            pricePerNight: 149,
            location: "Colorado",
            starRating: 4,
            averageRating: 4.8,
            reviewCount: 92,
            imageUrl: "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=400"
        }
    ];
    
    displayHotels(sampleHotels);
}

function getSamplePackageById(packageId) {
    const samplePackages = [
        {
            id: 1,
            title: "Bali Paradise Adventure",
            description: "Explore the beautiful beaches and temples of Bali",
            price: 899,
            duration: 7,
            destination: "Bali, Indonesia",
            averageRating: 4.8,
            reviewCount: 124,
            imageUrl: "https://images.unsplash.com/photo-1518548419970-58e3b4079ab2?w=400"
        },
        {
            id: 2,
            title: "Swiss Alps Experience",
            description: "Breathtaking mountain views and alpine adventures",
            price: 1299,
            duration: 10,
            destination: "Switzerland",
            averageRating: 4.9,
            reviewCount: 89,
            imageUrl: "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400"
        },
        {
            id: 3,
            title: "Tokyo Cultural Journey",
            description: "Immerse yourself in Japanese culture and cuisine",
            price: 1099,
            duration: 8,
            destination: "Tokyo, Japan",
            averageRating: 4.7,
            reviewCount: 156,
            imageUrl: "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=400"
        }
    ];
    
    return samplePackages.find(pkg => pkg.id == packageId);
}

function getSampleHotelById(hotelId) {
    const sampleHotels = [
        {
            id: 1,
            name: "Grand Paradise Resort",
            description: "Luxury beachfront resort with world-class amenities",
            pricePerNight: 299,
            location: "Maldives",
            starRating: 5,
            averageRating: 4.9,
            reviewCount: 234,
            imageUrl: "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=400"
        },
        {
            id: 2,
            name: "Urban Boutique Hotel",
            description: "Modern hotel in the heart of the city",
            pricePerNight: 189,
            location: "New York",
            starRating: 4,
            averageRating: 4.6,
            reviewCount: 178,
            imageUrl: "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=400"
        },
        {
            id: 3,
            name: "Mountain Lodge Retreat",
            description: "Cozy lodge with stunning mountain views",
            pricePerNight: 149,
            location: "Colorado",
            starRating: 4,
            averageRating: 4.8,
            reviewCount: 92,
            imageUrl: "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=400"
        }
    ];
    
    return sampleHotels.find(hotel => hotel.id == hotelId);
}

function displaySampleDestinations() {
    const sampleDestinations = [
        {
            name: "Paris",
            packageCount: 25,
            imageUrl: "https://images.unsplash.com/photo-1502602898536-47ad22581b52?w=400"
        },
        {
            name: "Tokyo",
            packageCount: 18,
            imageUrl: "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=400"
        },
        {
            name: "New York",
            packageCount: 32,
            imageUrl: "https://images.unsplash.com/photo-1496442226666-8d4d0e62e6e9?w=400"
        },
        {
            name: "London",
            packageCount: 21,
            imageUrl: "https://images.unsplash.com/photo-1513635269975-59663e0ac1ad?w=400"
        }
    ];
    
    displayDestinations(sampleDestinations, 'destinations-grid');
}

// Utility Functions
function generateStars(rating) {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);
    
    return '★'.repeat(fullStars) + 
           (hasHalfStar ? '☆' : '') + 
           '☆'.repeat(emptyStars);
}

function showLoading() {
    document.getElementById('loading')?.classList.remove('hidden');
}

function hideLoading() {
    document.getElementById('loading')?.classList.add('hidden');
}

function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${type === 'success' ? '#4CAF50' : type === 'error' ? '#f44336' : '#2196F3'};
        color: white;
        padding: 1rem 2rem;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        z-index: 9999;
        animation: slideIn 0.3s ease;
    `;
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    // Remove after 4 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => notification.remove(), 300);
    }, 4000);
}

// Action Functions
async function viewPackageDetails(packageId) {
    if (!authToken) {
        showNotification('Please log in to view package details', 'error');
        showLogin();
        return;
    }
    
    showLoading();
    try {
        // For now, use sample data since API endpoint doesn't exist
        const packageData = getSamplePackageById(packageId);
        if (packageData) {
            showPackageDetailsModal(packageData);
        } else {
            showNotification('Package not found', 'error');
        }
    } catch (error) {
        console.error('Error loading package details:', error);
        showNotification('Failed to load package details', 'error');
    } finally {
        hideLoading();
    }
}

async function viewHotelDetails(hotelId) {
    if (!authToken) {
        showNotification('Please log in to view hotel details', 'error');
        showLogin();
        return;
    }
    
    showLoading();
    try {
        // For now, use sample data since API endpoint doesn't exist
        const hotelData = getSampleHotelById(hotelId);
        if (hotelData) {
            showHotelDetailsModal(hotelData);
        } else {
            showNotification('Hotel not found', 'error');
        }
    } catch (error) {
        console.error('Error loading hotel details:', error);
        showNotification('Failed to load hotel details', 'error');
    } finally {
        hideLoading();
    }
}

function searchDestination(destination) {
    document.getElementById('destination').value = destination;
    document.querySelector('[data-tab="packages"]').click();
    searchPackages();
}

function loadMorePackages() {
    window.location.href = '/packages.html';
}

function loadMoreHotels() {
    window.location.href = '/hotels.html';
}

// Modal Functions
function showPackageDetailsModal(packageData) {
    // Create and show package details modal
    const modal = createDetailsModal('package', packageData);
    document.body.appendChild(modal);
}

function showHotelDetailsModal(hotelData) {
    // Create and show hotel details modal
    const modal = createDetailsModal('hotel', hotelData);
    document.body.appendChild(modal);
}

function createDetailsModal(type, data) {
    const modal = document.createElement('div');
    modal.className = 'modal active';
    modal.id = 'details-modal';
    
    const isPackage = type === 'package';
    
    modal.innerHTML = `
        <div class="modal-content" style="max-width: 600px;">
            <div class="modal-header">
                <h3>${data.title || data.name}</h3>
                <span class="close" onclick="closeModal('details-modal')">&times;</span>
            </div>
            <div class="modal-body">
                <img src="${data.imageUrl || (isPackage ? 'https://images.unsplash.com/photo-1469474968028-56623f02e42e?w=600' : 'https://images.unsplash.com/photo-1566073771259-6a8506099945?w=600')}" 
                     alt="${data.title || data.name}" style="width: 100%; height: 200px; object-fit: cover; border-radius: 8px; margin-bottom: 1rem;">
                <p>${data.description}</p>
                <div class="details-info">
                    <div class="info-row">
                        <strong>Price:</strong> $${isPackage ? data.price : data.pricePerNight}${isPackage ? '' : '/night'}
                    </div>
                    ${isPackage ? `<div class="info-row"><strong>Duration:</strong> ${data.duration} days</div>` : ''}
                    <div class="info-row">
                        <strong>Location:</strong> ${data.destination || data.location}
                    </div>
                    <div class="info-row">
                        <strong>Rating:</strong> ${generateStars(data.averageRating || 4.5)} (${data.reviewCount || 0} reviews)
                    </div>
                </div>
                <button class="btn-primary full-width" onclick="book${isPackage ? 'Package' : 'Hotel'}(${data.id})" style="margin-top: 1rem;">
                    Book Now
                </button>
            </div>
        </div>
    `;
    
    return modal;
}

async function bookPackage(packageId) {
    if (!authToken) {
        showNotification('Please log in to book packages', 'error');
        showLogin();
        return;
    }
    
    showLoading();
    try {
        const response = await fetch(`${API_BASE_URL}/api/booking/package`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({
                packageId: packageId,
                numberOfGuests: 2,
                bookingDate: new Date().toISOString().split('T')[0]
            })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            closeModal('details-modal');
            showNotification('Package booked successfully!', 'success');
        } else {
            showNotification(data.message || 'Booking failed', 'error');
        }
    } catch (error) {
        console.error('Booking error:', error);
        showNotification('Booking failed. Please try again.', 'error');
    } finally {
        hideLoading();
    }
}

async function bookHotel(hotelId) {
    if (!authToken) {
        showNotification('Please log in to book hotels', 'error');
        showLogin();
        return;
    }
    
    showLoading();
    try {
        const checkIn = new Date();
        checkIn.setDate(checkIn.getDate() + 7); // 7 days from now
        const checkOut = new Date();
        checkOut.setDate(checkOut.getDate() + 10); // 10 days from now
        
        const response = await fetch(`${API_BASE_URL}/api/booking/hotel`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({
                hotelId: hotelId,
                checkInDate: checkIn.toISOString().split('T')[0],
                checkOutDate: checkOut.toISOString().split('T')[0],
                numberOfGuests: 2,
                numberOfRooms: 1
            })
        });
        
        const data = await response.json();
        
        if (response.ok) {
            closeModal('details-modal');
            showNotification('Hotel booked successfully!', 'success');
        } else {
            showNotification(data.message || 'Booking failed', 'error');
        }
    } catch (error) {
        console.error('Booking error:', error);
        showNotification('Booking failed. Please try again.', 'error');
    } finally {
        hideLoading();
    }
}

// Add styles for notification animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    
    @keyframes slideOut {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(100%); opacity: 0; }
    }
    
    .info-row {
        margin: 0.5rem 0;
        padding: 0.5rem 0;
        border-bottom: 1px solid #eee;
    }
    
    .info-row:last-child {
        border-bottom: none;
    }
`;
document.head.appendChild(style);

// Add map functionality if on recommendations page
if (document.getElementById('map')) {
    const map = L.map('map').setView([40.7128, -74.0060], 10);
    const placeDetails = document.getElementById('place-details');

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    document.getElementById('get-recommendations-btn').addEventListener('click', async () => {
        try {
            const response = await fetch('/api/recommendations', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    location: document.getElementById('location-input').value,
                    interests: Array.from(document.querySelectorAll('input[name="interests"]:checked')).map(cb => cb.value)
                })
            });

            if (!response.ok) throw new Error('Failed to get recommendations');
            
            const recommendations = await response.json();

            // Clear existing markers
            map.eachLayer((layer) => {
                if (layer instanceof L.Marker) {
                    map.removeLayer(layer);
                }
            });

            // Add markers for recommended places
            recommendations.forEach(place => {
                const marker = L.marker([place.latitude, place.longitude])
                    .addTo(map)
                    .bindPopup(place.name);

                marker.on('click', () => showPlaceDetails(place));
            });

        } catch (error) {
            console.error('Error:', error);
            alert('Failed to get recommendations. Please try again.');
        }
    });

    // Show place details
    async function showPlaceDetails(place) {
        const placeNameElement = document.getElementById('place-name');
        const placeDescriptionElement = document.getElementById('place-description');
        const placeVideosElement = document.getElementById('place-videos');

        placeNameElement.textContent = place.name;
        placeDescriptionElement.innerHTML = place.description;
        placeDetails.classList.remove('hidden');

        try {
            const response = await fetch(`/api/places/${place.id}/videos`);
            if (!response.ok) throw new Error('Failed to fetch videos');
            
            const videos = await response.json();
            
            placeVideosElement.innerHTML = videos.map(video => `
                <div class="video-card">
                    <h4>${video.title}</h4>
                    <iframe 
                        width="100%" 
                        height="200" 
                        src="https://www.youtube.com/embed/${video.id}"
                        frameborder="0" 
                        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" 
                        allowfullscreen>
                    </iframe>
                </div>
            `).join('');
        } catch (error) {
            console.error('Error fetching videos:', error);
            placeVideosElement.innerHTML = '<p>Failed to load videos</p>';
        }
    }
}
