export async function load(): Promise<Response> {
  return await fetch('/api/v1/orders');
}
