// Packages Page JavaScript
let allPackages = [];
let filteredPackages = [];
let currentPage = 1;
const packagesPerPage = 12;

// Initialize packages page
document.addEventListener('DOMContentLoaded', function() {
    initializePackagesPage();
    setupPackageEventListeners();
    loadAllPackages();
});

function initializePackagesPage() {
    // Check authentication status
    if (authToken) {
        checkAuthStatus();
    }
    
    // Setup mobile menu
    setupMobileMenu();
    
    // Setup search
    setupPackageSearch();
}

function setupPackageEventListeners() {
    // Search input
    document.getElementById('search-input').addEventListener('input', debounce(performSearch, 300));
    
    // Filter changes
    document.getElementById('price-filter').addEventListener('change', applyFilters);
    document.getElementById('duration-filter').addEventListener('change', applyFilters);
    document.getElementById('category-filter').addEventListener('change', applyFilters);
}

function setupPackageSearch() {
    // Enable search on Enter key
    document.getElementById('search-input').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            performSearch();
        }
    });
}

// Load packages data
async function loadAllPackages() {
    try {
        showLoading();
        
        const response = await fetch(`${API_BASE_URL}/api/public/packages`);
        if (response.ok) {
            const data = await response.json();
            allPackages = data.packages || data || [];
        } else {
            // Use sample data if API fails
            allPackages = getSamplePackages();
        }
        
        filteredPackages = [...allPackages];
        displayPackages();
        updateResultsCount();
        
    } catch (error) {
        console.error('Error loading packages:', error);
        allPackages = getSamplePackages();
        filteredPackages = [...allPackages];
        displayPackages();
        updateResultsCount();
    } finally {
        hideLoading();
    }
}

function getSamplePackages() {
    return [
        {
            id: 1,
            title: "Bali Paradise Adventure",
            description: "Explore the beautiful beaches, temples, and culture of Bali with this comprehensive 7-day package.",
            price: 899,
            duration: 7,
            destination: "Bali, Indonesia",
            category: "beach",
            averageRating: 4.8,
            reviewCount: 124,
            imageUrl: "https://images.unsplash.com/photo-1518548419970-58e3b4079ab2?w=400",
            features: ["Beach Access", "Temple Tours", "Cultural Activities", "Airport Transfer"]
        },
        {
            id: 2,
            title: "Swiss Alps Mountain Experience",
            description: "Breathtaking mountain views, alpine adventures, and cozy chalets in the heart of Switzerland.",
            price: 1299,
            duration: 10,
            destination: "Switzerland",
            category: "adventure",
            averageRating: 4.9,
            reviewCount: 89,
            imageUrl: "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400",
            features: ["Mountain Hiking", "Cable Car Rides", "Alpine Villages", "Scenic Railways"]
        },
        {
            id: 3,
            title: "Tokyo Cultural Journey",
            description: "Immerse yourself in Japanese culture, cuisine, and modern city life in vibrant Tokyo.",
            price: 1099,
            duration: 8,
            destination: "Tokyo, Japan",
            category: "culture",
            averageRating: 4.7,
            reviewCount: 156,
            imageUrl: "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=400",
            features: ["Cultural Sites", "Food Tours", "Traditional Shows", "Modern Attractions"]
        },
        {
            id: 4,
            title: "Paris Romantic Getaway",
            description: "Experience the city of love with romantic walks, fine dining, and iconic landmarks.",
            price: 1199,
            duration: 6,
            destination: "Paris, France",
            category: "city",
            averageRating: 4.6,
            reviewCount: 203,
            imageUrl: "https://images.unsplash.com/photo-1502602898536-47ad22581b52?w=400",
            features: ["Eiffel Tower", "Seine River Cruise", "Louvre Museum", "Fine Dining"]
        },
        {
            id: 5,
            title: "African Safari Adventure",
            description: "Witness the Big Five and experience the raw beauty of African wildlife in their natural habitat.",
            price: 2299,
            duration: 12,
            destination: "Kenya & Tanzania",
            category: "nature",
            averageRating: 4.9,
            reviewCount: 78,
            imageUrl: "https://images.unsplash.com/photo-1516426122078-c23e76319801?w=400",
            features: ["Wildlife Safari", "Game Drives", "Luxury Lodges", "Cultural Visits"]
        },
        {
            id: 6,
            title: "Mediterranean Cruise",
            description: "Sail through crystal-clear waters visiting multiple Mediterranean destinations.",
            price: 1599,
            duration: 9,
            destination: "Mediterranean",
            category: "cruise",
            averageRating: 4.5,
            reviewCount: 167,
            imageUrl: "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=400",
            features: ["Multiple Destinations", "All-Inclusive", "Ocean Views", "Onboard Activities"]
        },
        {
            id: 7,
            title: "New York City Explorer",
            description: "Discover the Big Apple with its iconic skyline, Broadway shows, and world-class museums.",
            price: 899,
            duration: 5,
            destination: "New York, USA",
            category: "city",
            averageRating: 4.4,
            reviewCount: 234,
            imageUrl: "https://images.unsplash.com/photo-1496442226666-8d4d0e62e6e9?w=400",
            features: ["Broadway Shows", "Museums", "Central Park", "Statue of Liberty"]
        },
        {
            id: 8,
            title: "Amazon Rainforest Expedition",
            description: "Explore the world's largest rainforest and discover incredible biodiversity.",
            price: 1899,
            duration: 11,
            destination: "Amazon, Brazil",
            category: "nature",
            averageRating: 4.8,
            reviewCount: 92,
            imageUrl: "https://images.unsplash.com/photo-1516426122078-c23e76319801?w=400",
            features: ["Jungle Trekking", "River Cruises", "Wildlife Spotting", "Indigenous Culture"]
        }
    ];
}

// Display functions
function displayPackages() {
    const container = document.getElementById('packages-grid');
    const startIndex = (currentPage - 1) * packagesPerPage;
    const endIndex = startIndex + packagesPerPage;
    const packagesToShow = filteredPackages.slice(0, endIndex);
    
    if (packagesToShow.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-search"></i>
                <h3>No packages found</h3>
                <p>Try adjusting your search criteria or filters</p>
            </div>
        `;
        document.getElementById('load-more-container').style.display = 'none';
        return;
    }
    
    container.innerHTML = packagesToShow.map(pkg => createPackageCard(pkg)).join('');
    
    // Show/hide load more button
    const loadMoreContainer = document.getElementById('load-more-container');
    if (endIndex >= filteredPackages.length) {
        loadMoreContainer.style.display = 'none';
    } else {
        loadMoreContainer.style.display = 'block';
    }
}

function createPackageCard(pkg) {
    return `
        <div class="package-card" data-package-id="${pkg.id}">
            <div class="card-image">
                <img src="${pkg.imageUrl || 'https://images.unsplash.com/photo-1469474968028-56623f02e42e?w=400'}" 
                     alt="${pkg.title}" onerror="this.src='https://images.unsplash.com/photo-1469474968028-56623f02e42e?w=400'">
                <div class="card-badge">${pkg.duration} days</div>
            </div>
            <div class="card-content">
                <h3 class="card-title">${pkg.title}</h3>
                <p class="card-destination">
                    <i class="fas fa-map-marker-alt"></i> ${pkg.destination}
                </p>
                <p class="card-description">${pkg.description}</p>
                <div class="card-rating">
                    <div class="stars">${generateStars(pkg.averageRating || 4.5)}</div>
                    <span class="rating-text">(${pkg.reviewCount || 0} reviews)</span>
                </div>
                <div class="card-features">
                    ${(pkg.features || []).slice(0, 3).map(feature => 
                        `<span class="feature-tag">${feature}</span>`
                    ).join('')}
                </div>
                <div class="card-footer">
                    <div class="card-price">
                        <span class="price-label">From</span>
                        <span class="price-amount">$${pkg.price}</span>
                        <span class="price-person">per person</span>
                    </div>
                    <button class="btn-primary" onclick="showPackageDetails(${pkg.id})">
                        View Details
                    </button>
                </div>
            </div>
        </div>
    `;
}

// Search and filter functions
function performSearch() {
    const searchTerm = document.getElementById('search-input').value.toLowerCase();
    
    if (searchTerm.trim() === '') {
        filteredPackages = [...allPackages];
    } else {
        filteredPackages = allPackages.filter(pkg => 
            pkg.title.toLowerCase().includes(searchTerm) ||
            pkg.description.toLowerCase().includes(searchTerm) ||
            pkg.destination.toLowerCase().includes(searchTerm) ||
            (pkg.features && pkg.features.some(feature => feature.toLowerCase().includes(searchTerm)))
        );
    }
    
    currentPage = 1;
    displayPackages();
    updateResultsCount();
}

function applyFilters() {
    const priceFilter = document.getElementById('price-filter').value;
    const durationFilter = document.getElementById('duration-filter').value;
    const categoryFilter = document.getElementById('category-filter').value;
    const searchTerm = document.getElementById('search-input').value.toLowerCase();
    
    let filtered = [...allPackages];
    
    // Apply search filter
    if (searchTerm.trim() !== '') {
        filtered = filtered.filter(pkg => 
            pkg.title.toLowerCase().includes(searchTerm) ||
            pkg.description.toLowerCase().includes(searchTerm) ||
            pkg.destination.toLowerCase().includes(searchTerm) ||
            (pkg.features && pkg.features.some(feature => feature.toLowerCase().includes(searchTerm)))
        );
    }
    
    // Apply price filter
    if (priceFilter) {
        if (priceFilter === '2000+') {
            filtered = filtered.filter(pkg => pkg.price >= 2000);
        } else {
            const [min, max] = priceFilter.split('-').map(Number);
            filtered = filtered.filter(pkg => pkg.price >= min && pkg.price <= max);
        }
    }
    
    // Apply duration filter
    if (durationFilter) {
        if (durationFilter === '15+') {
            filtered = filtered.filter(pkg => pkg.duration >= 15);
        } else {
            const [min, max] = durationFilter.split('-').map(Number);
            filtered = filtered.filter(pkg => pkg.duration >= min && pkg.duration <= max);
        }
    }
    
    // Apply category filter
    if (categoryFilter) {
        filtered = filtered.filter(pkg => pkg.category === categoryFilter);
    }
    
    filteredPackages = filtered;
    currentPage = 1;
    displayPackages();
    updateResultsCount();
}

function sortPackages() {
    const sortBy = document.getElementById('sort-select').value;
    
    switch(sortBy) {
        case 'price-low':
            filteredPackages.sort((a, b) => a.price - b.price);
            break;
        case 'price-high':
            filteredPackages.sort((a, b) => b.price - a.price);
            break;
        case 'duration-short':
            filteredPackages.sort((a, b) => a.duration - b.duration);
            break;
        case 'duration-long':
            filteredPackages.sort((a, b) => b.duration - a.duration);
            break;
        case 'rating':
            filteredPackages.sort((a, b) => (b.averageRating || 0) - (a.averageRating || 0));
            break;
        default:
            // Featured - keep original order or sort by review count
            filteredPackages.sort((a, b) => (b.reviewCount || 0) - (a.reviewCount || 0));
            break;
    }
    
    currentPage = 1;
    displayPackages();
}

function loadMorePackages() {
    currentPage++;
    displayPackages();
}

function updateResultsCount() {
    const count = filteredPackages.length;
    const countElement = document.getElementById('results-count');
    
    if (count === 0) {
        countElement.textContent = 'No packages found';
    } else if (count === 1) {
        countElement.textContent = '1 package found';
    } else {
        countElement.textContent = `${count} packages found`;
    }
}

// Package details modal
async function showPackageDetails(packageId) {
    const package = filteredPackages.find(p => p.id === packageId) || 
                   allPackages.find(p => p.id === packageId);
    
    if (!package) {
        showNotification('Package not found', 'error');
        return;
    }
    
    const modal = document.getElementById('package-detail-modal');
    const title = document.getElementById('modal-package-title');
    const content = document.getElementById('package-detail-content');
    
    title.textContent = package.title;
    
    content.innerHTML = `
        <div class="package-detail-grid">
            <div class="package-detail-image">
                <img src="${package.imageUrl || 'https://images.unsplash.com/photo-1469474968028-56623f02e42e?w=600'}" 
                     alt="${package.title}">
            </div>
            <div class="package-detail-info">
                <div class="detail-section">
                    <h4><i class="fas fa-map-marker-alt"></i> Destination</h4>
                    <p>${package.destination}</p>
                </div>
                
                <div class="detail-section">
                    <h4><i class="fas fa-calendar"></i> Duration</h4>
                    <p>${package.duration} days</p>
                </div>
                
                <div class="detail-section">
                    <h4><i class="fas fa-star"></i> Rating</h4>
                    <div class="rating-display">
                        <span class="stars">${generateStars(package.averageRating || 4.5)}</span>
                        <span class="rating-score">${(package.averageRating || 4.5).toFixed(1)}</span>
                        <span class="rating-reviews">(${package.reviewCount || 0} reviews)</span>
                    </div>
                </div>
                
                <div class="detail-section">
                    <h4><i class="fas fa-dollar-sign"></i> Price</h4>
                    <div class="price-display">
                        <span class="price-amount">$${package.price}</span>
                        <span class="price-person">per person</span>
                    </div>
                </div>
            </div>
        </div>
        
        <div class="package-description">
            <h4>About This Package</h4>
            <p>${package.description}</p>
        </div>
        
        <div class="package-features">
            <h4>What's Included</h4>
            <div class="features-grid">
                ${(package.features || ['Accommodation', 'Transportation', 'Guided Tours', 'Meals']).map(feature => 
                    `<div class="feature-item">
                        <i class="fas fa-check"></i>
                        <span>${feature}</span>
                    </div>`
                ).join('')}
            </div>
        </div>
        
        <div class="package-actions">
            <button class="btn-outline" onclick="addToWishlist(${package.id})">
                <i class="fas fa-heart"></i> Add to Wishlist
            </button>
            <button class="btn-primary" onclick="bookPackage(${package.id})">
                <i class="fas fa-calendar-check"></i> Book Now
            </button>
        </div>
    `;
    
    modal.classList.add('active');
}

// Package actions
async function bookPackage(packageId) {
    if (!authToken) {
        closeModal('package-detail-modal');
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
            closeModal('package-detail-modal');
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

function addToWishlist(packageId) {
    if (!authToken) {
        showNotification('Please log in to add to wishlist', 'error');
        showLogin();
        return;
    }
    
    // Implement wishlist functionality
    showNotification('Added to wishlist!', 'success');
}

// Utility functions
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Add package-specific CSS
const packageStyles = document.createElement('style');
packageStyles.textContent = `
    .page-header {
        background: linear-gradient(135deg, rgba(44, 90, 160, 0.9), rgba(243, 156, 18, 0.7)),
                    url('https://images.unsplash.com/photo-1469474968028-56623f02e42e?w=1200') center/cover;
        padding: 8rem 0 4rem;
        text-align: center;
        color: white;
        margin-top: 70px;
    }
    
    .page-header h1 {
        font-size: 3rem;
        margin-bottom: 1rem;
        text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
    }
    
    .page-header p {
        font-size: 1.3rem;
        opacity: 0.9;
    }
    
    .search-filters {
        padding: 3rem 0;
        background: white;
        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }
    
    .filters-container {
        display: flex;
        gap: 1rem;
        align-items: center;
        flex-wrap: wrap;
    }
    
    .search-box {
        position: relative;
        flex: 2;
        min-width: 300px;
    }
    
    .search-box i {
        position: absolute;
        left: 1rem;
        top: 50%;
        transform: translateY(-50%);
        color: #666;
    }
    
    .search-box input {
        width: 100%;
        padding: 1rem 1rem 1rem 3rem;
        border: 2px solid #e9ecef;
        border-radius: 50px;
        font-size: 1rem;
        transition: border-color 0.3s ease;
    }
    
    .search-box input:focus {
        outline: none;
        border-color: #2c5aa0;
    }
    
    .filter-group {
        flex: 1;
        min-width: 150px;
    }
    
    .filter-group select {
        width: 100%;
        padding: 1rem;
        border: 2px solid #e9ecef;
        border-radius: 8px;
        font-size: 1rem;
        background: white;
        cursor: pointer;
    }
    
    .packages-section {
        padding: 4rem 0;
        background: #f8f9fa;
    }
    
    .section-actions {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 2rem;
        padding: 1.5rem;
        background: white;
        border-radius: 10px;
        box-shadow: 0 2px 10px rgba(0,0,0,0.05);
    }
    
    .results-info {
        font-weight: 600;
        color: #2c5aa0;
    }
    
    .sort-options {
        display: flex;
        align-items: center;
        gap: 1rem;
    }
    
    .sort-options label {
        font-weight: 600;
        color: #333;
    }
    
    .sort-options select {
        padding: 0.5rem;
        border: 2px solid #e9ecef;
        border-radius: 8px;
        background: white;
    }
    
    .card-badge {
        position: absolute;
        top: 1rem;
        right: 1rem;
        background: rgba(44, 90, 160, 0.9);
        color: white;
        padding: 0.5rem 1rem;
        border-radius: 20px;
        font-size: 0.8rem;
        font-weight: 600;
    }
    
    .card-destination {
        color: #666;
        margin-bottom: 1rem;
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }
    
    .card-destination i {
        color: #f39c12;
    }
    
    .card-footer {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-top: 1rem;
        padding-top: 1rem;
        border-top: 1px solid #e9ecef;
    }
    
    .price-label {
        font-size: 0.8rem;
        color: #666;
        display: block;
    }
    
    .price-amount {
        font-size: 1.5rem;
        font-weight: 700;
        color: #f39c12;
    }
    
    .price-person {
        font-size: 0.8rem;
        color: #666;
    }
    
    .package-modal {
        max-width: 800px;
        max-height: 90vh;
        overflow-y: auto;
    }
    
    .package-detail-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 2rem;
        margin-bottom: 2rem;
    }
    
    .package-detail-image img {
        width: 100%;
        height: 250px;
        object-fit: cover;
        border-radius: 10px;
    }
    
    .detail-section {
        margin-bottom: 1.5rem;
        padding-bottom: 1rem;
        border-bottom: 1px solid #e9ecef;
    }
    
    .detail-section:last-child {
        border-bottom: none;
    }
    
    .detail-section h4 {
        color: #2c5aa0;
        margin-bottom: 0.5rem;
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }
    
    .rating-display,
    .price-display {
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }
    
    .rating-score {
        font-weight: 600;
        color: #f39c12;
    }
    
    .rating-reviews {
        color: #666;
        font-size: 0.9rem;
    }
    
    .package-description,
    .package-features {
        margin-bottom: 2rem;
    }
    
    .package-description h4,
    .package-features h4 {
        color: #2c5aa0;
        margin-bottom: 1rem;
    }
    
    .features-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 1rem;
    }
    
    .feature-item {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.5rem;
        background: #f8f9fa;
        border-radius: 8px;
    }
    
    .feature-item i {
        color: #28a745;
    }
    
    .package-actions {
        display: flex;
        gap: 1rem;
        padding-top: 1rem;
        border-top: 1px solid #e9ecef;
    }
    
    .empty-state {
        grid-column: 1 / -1;
        text-align: center;
        padding: 4rem 2rem;
        color: #666;
    }
    
    .empty-state i {
        font-size: 4rem;
        margin-bottom: 1rem;
        color: #e9ecef;
    }
    
    .empty-state h3 {
        margin-bottom: 0.5rem;
        color: #333;
    }
    
    @media (max-width: 768px) {
        .page-header h1 {
            font-size: 2rem;
        }
        
        .filters-container {
            flex-direction: column;
            align-items: stretch;
        }
        
        .section-actions {
            flex-direction: column;
            gap: 1rem;
            align-items: stretch;
        }
        
        .card-footer {
            flex-direction: column;
            gap: 1rem;
        }
        
        .package-detail-grid {
            grid-template-columns: 1fr;
        }
        
        .package-actions {
            flex-direction: column;
        }
    }
`;

document.head.appendChild(packageStyles);
