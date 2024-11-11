namespace Entities;


// This class contains a few custom exceptions, so I can better distinguish between errors.
// Each custom exception just takes a message, and passes it to the base class.

public class NotFoundException(string message) : Exception(message);

public class ValidationException(string message) : Exception(message);

