using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;using WorkForce360.Api.Data;using WorkForce360.Api.Models;
namespace WorkForce360.Api.Controllers;
[ApiController,Route("api/resources"),Authorize(Roles="OperationsManager")]
public class TripResourcesController(TripDbContext db):ControllerBase
{
 [HttpGet("guides")]public async Task<IActionResult> Guides()=>Ok(await db.Guides.AsNoTracking().Where(x=>!x.IsDeleted).OrderBy(x=>x.Name).ToListAsync());
 [HttpPost("guides")]public async Task<IActionResult> AddGuide(Guide x){x.Id=Guid.NewGuid();db.Add(x);await db.SaveChangesAsync();return Created($"/api/resources/guides/{x.Id}",x);}
 [HttpPut("guides/{id:guid}")]public async Task<IActionResult> EditGuide(Guid id,Guide x){var v=await db.Guides.FindAsync(id);if(v is null)return NotFound();v.Name=x.Name;v.DayRateLkr=x.DayRateLkr;v.MaxPax=x.MaxPax;v.Languages=x.Languages;v.IsActive=x.IsActive;await db.SaveChangesAsync();return NoContent();}
 [HttpDelete("guides/{id:guid}")]public async Task<IActionResult> DeleteGuide(Guid id){var x=await db.Guides.FindAsync(id);if(x is null)return NotFound();x.IsDeleted=true;await db.SaveChangesAsync();return NoContent();}
 [HttpGet("vehicles")]public async Task<IActionResult> Vehicles()=>Ok(await db.Vehicles.AsNoTracking().Where(x=>!x.IsDeleted).OrderBy(x=>x.RegistrationNo).ToListAsync());
 [HttpPost("vehicles")]public async Task<IActionResult> AddVehicle(Vehicle x){x.Id=Guid.NewGuid();db.Add(x);await db.SaveChangesAsync();return Created($"/api/resources/vehicles/{x.Id}",x);}
 [HttpPut("vehicles/{id:guid}")]public async Task<IActionResult> EditVehicle(Guid id,Vehicle x){var v=await db.Vehicles.FindAsync(id);if(v is null)return NotFound();v.RegistrationNo=x.RegistrationNo;v.Type=x.Type;v.Seats=x.Seats;v.RatePerKmLkr=x.RatePerKmLkr;v.IsActive=x.IsActive;await db.SaveChangesAsync();return NoContent();}
 [HttpDelete("vehicles/{id:guid}")]public async Task<IActionResult> DeleteVehicle(Guid id){var x=await db.Vehicles.FindAsync(id);if(x is null)return NotFound();x.IsDeleted=true;await db.SaveChangesAsync();return NoContent();}
 [HttpGet("hotels")]public async Task<IActionResult> Hotels()=>Ok(await db.Hotels.AsNoTracking().Include(x=>x.RoomTypes).Where(x=>!x.IsDeleted).OrderBy(x=>x.City).ThenBy(x=>x.Name).ToListAsync());
 [HttpPost("hotels")]public async Task<IActionResult> AddHotel(Hotel x){x.Id=Guid.NewGuid();db.Add(x);await db.SaveChangesAsync();return Created($"/api/resources/hotels/{x.Id}",x);}
 [HttpDelete("hotels/{id:guid}")]public async Task<IActionResult> DeleteHotel(Guid id){var x=await db.Hotels.FindAsync(id);if(x is null)return NotFound();x.IsDeleted=true;await db.SaveChangesAsync();return NoContent();}
 [HttpGet("availability")]public async Task<IActionResult> Availability(string type,DateOnly from,DateOnly to){if(to<from)return ValidationProblem("Invalid date range.");var held=await db.ResourceHolds.AsNoTracking().Where(x=>x.ResourceType==type&&x.Status==HoldStatus.Held&&x.FromDate<=to&&x.ToDate>=from).Select(x=>x.ResourceId).ToListAsync();return type.ToLower() switch{"guide"=>Ok(await db.Guides.Where(x=>x.IsActive&&!x.IsDeleted&&!held.Contains(x.Id)).ToListAsync()),"vehicle"=>Ok(await db.Vehicles.Where(x=>x.IsActive&&!x.IsDeleted&&!held.Contains(x.Id)).ToListAsync()),"room"=>Ok(await db.RoomTypes.Where(x=>!held.Contains(x.Id)).ToListAsync()),_=>BadRequest(new ProblemDetails{Title="Type must be guide, vehicle or room"})};}
}

[ApiController,Route("api/attractions"),Authorize]
public class AttractionsController(TripDbContext db):ControllerBase
{
 [HttpGet]public async Task<IActionResult> Get(string? city)=>Ok(await db.Attractions.AsNoTracking().Where(x=>!x.IsDeleted&&(city==null||x.City==city)).OrderBy(x=>x.City).ThenBy(x=>x.Name).ToListAsync());
 [HttpPost,Authorize(Roles="OperationsManager")]public async Task<IActionResult> Create(Attraction x){x.Id=Guid.NewGuid();db.Add(x);await db.SaveChangesAsync();return Created($"/api/attractions/{x.Id}",x);}
 [HttpPut("{id:guid}"),Authorize(Roles="OperationsManager")]public async Task<IActionResult> Update(Guid id,Attraction x){var a=await db.Attractions.FindAsync(id);if(a is null)return NotFound();a.Name=x.Name;a.City=x.City;a.Category=x.Category;a.DurationMinutes=x.DurationMinutes;a.EntryFeeLkr=x.EntryFeeLkr;a.Latitude=x.Latitude;a.Longitude=x.Longitude;await db.SaveChangesAsync();return NoContent();}
 [HttpDelete("{id:guid}"),Authorize(Roles="OperationsManager")]public async Task<IActionResult> Delete(Guid id){var a=await db.Attractions.FindAsync(id);if(a is null)return NotFound();a.IsDeleted=true;await db.SaveChangesAsync();return NoContent();}
}
