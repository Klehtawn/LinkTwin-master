#if UNITY_METRO || UNITY_WP8

public class WinStoreProduct
{
	public string productIdentifier { get; private set; }
	public string title { get; private set; }
	public string description { get; private set; }
	public string price { get; private set; }
	public string priceCurrencyCode { get; private set; }

	public WinStoreProduct(string productID, string productTitle, string productDescription, string productPrice, string productCurrencyCode)
	{
		productIdentifier = productID;

		title = productTitle;

		description = productDescription;

		price = productPrice;

		priceCurrencyCode = productCurrencyCode;
	}

	public override string ToString()
	{
		return string.Format( "[WinStoreProduct[: productId: {0}, title: {1}, price: {2}, description: {3}]", productIdentifier, title, price, description );
	}
}
#endif