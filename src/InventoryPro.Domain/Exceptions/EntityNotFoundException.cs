namespace InventoryPro.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityName, object entityId)
        : base($"Entity '{entityName}' with id '{entityId}' was not found.", "ENTITY_NOT_FOUND")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public static EntityNotFoundException For<T>(object entityId) where T : class
    {
        return new EntityNotFoundException(typeof(T).Name, entityId);
    }
}
