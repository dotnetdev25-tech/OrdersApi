public sealed record OrderItemUpsertDto(
    int product_id,
    int quantity,
    decimal unit_price
);
