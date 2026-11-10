export interface ProblemDetails {
  status: number;
  code: string;
  title: string;
  errors?: { path: string; code: string; message: string }[];
}

export async function postJson(url: string, body: unknown): Promise<{ ok: true } | { ok: false; problem: ProblemDetails }> {
  const response = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  if (response.ok) {
    return { ok: true };
  }
  return { ok: false, problem: (await response.json()) as ProblemDetails };
}
