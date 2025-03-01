CREATE DATABASE IF NOT EXISTS TravelRecommendation;
USE TravelRecommendation;

CREATE TABLE IF NOT EXISTS Places (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(255) NOT NULL,
    Description TEXT NOT NULL,
    Latitude DOUBLE NOT NULL,
    Longitude DOUBLE NOT NULL,
    Categories TEXT NOT NULL,
    BestSeasons TEXT NOT NULL,
    RecommendedDuration INT NOT NULL,
    Rating DOUBLE NOT NULL
);

CREATE TABLE IF NOT EXISTS Videos (
    Id VARCHAR(50) PRIMARY KEY,
    PlaceId INT,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    ThumbnailUrl VARCHAR(255),
    PublishedAt DATETIME NOT NULL,
    FOREIGN KEY (PlaceId) REFERENCES Places(Id)
);

-- Sample data for Places
INSERT INTO Places (Name, Description, Latitude, Longitude, Categories, BestSeasons, RecommendedDuration, Rating) VALUES
('Ladakh', 'Known for its stunning landscapes, Buddhist monasteries, and high-altitude desert.', 34.2268, 77.5619, 'hiking,culture,wildlife', 'summer', 7, 4.8),
('Rishikesh', 'The yoga capital of the world, known for adventure sports and spiritual activities.', 30.0869, 78.2676, 'water-sports,culture', 'winter,summer', 3, 4.6),
('Kerala Backwaters', 'A network of lagoons, lakes, and canals parallel to the Arabian Sea coast.', 9.4981, 76.3388, 'water-sports,culture', 'winter', 2, 4.7),
('Valley of Flowers', 'A national park known for its meadows of endemic alpine flowers.', 30.7283, 79.6050, 'hiking,wildlife', 'summer,monsoon', 4, 4.9),
('Ranthambore', 'One of the largest national parks in northern India, famous for tiger sightings.', 26.0173, 76.5026, 'wildlife', 'winter', 3, 4.5);
