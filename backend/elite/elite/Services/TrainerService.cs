// Services/TrainerService.cs
using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Microsoft.EntityFrameworkCore;

namespace elite.Services
{
    public class TrainerService : ITrainerService
    {
        private readonly GymDbContext _context;
        private readonly ILogger<TrainerService> _logger;
        private readonly IImageService _imageService;

        public TrainerService(GymDbContext context, ILogger<TrainerService> logger, IImageService imageService)
        {
            _context = context;
            _logger = logger;
            _imageService = imageService;
        }

        public async Task<IEnumerable<TrainerDto>> GetAllTrainersAsync()
        {
            return await _context.Trainers
                .Select(t => new TrainerDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Specialization = t.Specialization,
                    ExperienceYears = t.ExperienceYears,
                    Certifications = t.Certifications,
                    Bio = t.Bio,
                    ImageUrl = t.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<TrainerDto> GetTrainerByIdAsync(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null) throw new ArgumentException("Trainer not found");

            return new TrainerDto
            {
                Id = trainer.Id,
                Name = trainer.Name,
                Specialization = trainer.Specialization,
                ExperienceYears = trainer.ExperienceYears,
                Certifications = trainer.Certifications,
                Bio = trainer.Bio,
                ImageUrl = trainer.ImageUrl
            };
        }

        public async Task<TrainerDto> CreateTrainerAsync(TrainerCreateDto trainerCreateDto)
        {
            try
            {
                string imageUrl = null;

                if (trainerCreateDto.ImageFile != null)
                {
                    try
                    {
                        imageUrl = await _imageService.UploadImageAsync(trainerCreateDto.ImageFile, "trainers");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image for trainer");
                        // Continue without image if upload fails
                    }
                }

                var trainer = new Trainer
                {
                    Name = trainerCreateDto.Name,
                    Specialization = trainerCreateDto.Specialization,
                    ExperienceYears = trainerCreateDto.ExperienceYears,
                    Certifications = trainerCreateDto.Certifications,
                    Bio = trainerCreateDto.Bio,
                    ImageUrl = imageUrl
                };

                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();

                return new TrainerDto
                {
                    Id = trainer.Id,
                    Name = trainer.Name,
                    Specialization = trainer.Specialization,
                    ExperienceYears = trainer.ExperienceYears,
                    Certifications = trainer.Certifications,
                    Bio = trainer.Bio,
                    ImageUrl = trainer.ImageUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trainer");
                throw;
            }
        }

        // Updated method with TrainerUpdateDto
        public async Task<TrainerDto> UpdateTrainerAsync(int id, TrainerUpdateDto trainerUpdateDto)
        {
            try
            {
                var trainer = await _context.Trainers.FindAsync(id);
                if (trainer == null) throw new ArgumentException("Trainer not found");

                // Handle image update ONLY if a new image is provided
                if (trainerUpdateDto.ImageFile != null && trainerUpdateDto.ImageFile.Length > 0)
                {
                    try
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(trainer.ImageUrl))
                        {
                            _imageService.DeleteImage(trainer.ImageUrl);
                        }

                        // Upload new image
                        trainer.ImageUrl = await _imageService.UploadImageAsync(trainerUpdateDto.ImageFile, "trainers");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating image for trainer");
                        // Keep existing image if upload fails
                    }
                }
                // If no new image is provided, keep the existing image URL

                // Update other properties
                trainer.Name = trainerUpdateDto.Name;
                trainer.Specialization = trainerUpdateDto.Specialization;
                trainer.ExperienceYears = trainerUpdateDto.ExperienceYears;
                trainer.Certifications = trainerUpdateDto.Certifications;
                trainer.Bio = trainerUpdateDto.Bio;

                await _context.SaveChangesAsync();

                return new TrainerDto
                {
                    Id = trainer.Id,
                    Name = trainer.Name,
                    Specialization = trainer.Specialization,
                    ExperienceYears = trainer.ExperienceYears,
                    Certifications = trainer.Certifications,
                    Bio = trainer.Bio,
                    ImageUrl = trainer.ImageUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trainer");
                throw;
            }
        }

        public async Task<bool> DeleteTrainerAsync(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null) throw new ArgumentException("Trainer not found");

            // Delete associated image
            if (!string.IsNullOrEmpty(trainer.ImageUrl))
            {
                _imageService.DeleteImage(trainer.ImageUrl);
            }

            _context.Trainers.Remove(trainer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<ClassDto>> GetTrainerClassesAsync(int trainerId)
        {
            return await _context.Classes
                .Where(c => c.TrainerId == trainerId)
                .Select(c => new ClassDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Duration = c.Duration,
                    TrainerId = c.TrainerId,
                    TrainerName = c.Trainer.Name,
                    MaxCapacity = c.MaxCapacity,
                    Price = c.Price
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<OnlineSessionDto>> GetTrainerSessionsAsync(int trainerId)
        {
            return await _context.OnlineSessions
                .Where(os => os.TrainerId == trainerId)
                .Select(os => new OnlineSessionDto
                {
                    Id = os.Id,
                    Name = os.Name,
                    Description = os.Description,
                    Duration = os.Duration,
                    TrainerId = os.TrainerId,
                    TrainerName = os.Trainer.Name,
                    Price = os.Price
                })
                .ToListAsync();
        }
    }
}