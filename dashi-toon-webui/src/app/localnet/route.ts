import { NextResponse } from "next/server";
import * as env from "@/utils/env";
export async function POST(req: Request) {
    if (process.env.NODE_ENV !== "development")
        return NextResponse.json({ base: null });

    const url = new URL(await req.text());

    if (url.hostname === "localhost" || url.hostname === "127.0.0.1") {
        return NextResponse.json({ base: null });
    }

    const envUrl = new URL(env.getBackendHost()!);
    const newBase = "http://" + url.hostname + ":" + envUrl.port;

    return NextResponse.json({ base: newBase });
}
