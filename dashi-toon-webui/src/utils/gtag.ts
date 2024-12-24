import { SearchOptions } from "./api/series";
import { STATUS_CONFIG_2 } from "./consts";

type Event = Gtag.GtagCommands["event"];

export function event(name: Event[0], args: Event[1]) {
    setTimeout(() => {
        try {
            gtag("event", name, args);
        } catch (e) {
            console.error(e);
        }
    }, 1000);
}

export function eventSearch(opt: SearchOptions) {
    const genres = (opt.genres?.length ?? 0 > 0) ? `genres: ${opt.genres?.join(",")}` : "";
    const status = opt.status ? `status: ${opt.status.map((x) => STATUS_CONFIG_2[parseInt(x)]).join(",")}` : "";

    const term = `${opt.term} ${genres} ${status}`;
    event("search", {
        search_term: term,
    });
}

export function eventShare(seriesId: string, type: "NOVEL" | "COMIC") {
    event("share", {
        method: "links",
        content_type: type,
        item_id: seriesId,
    });
}

export function eventFollow(seriesId: string) {
    event("follow", {
        seriesId,
    });
}

export function eventUnfollow(seriesId: string) {
    event("unfollow", {
        seriesId,
    });
}

export function unlockChapter(
    val: number,
    type: "KanaCoin" | "KanaGold",
    chapterId: string,
    seriesId: string,
    volumeId: string,
) {
    event("spend_virtual_currency", {
        value: val,
        virtual_currency_name: type,
        item_name: `Unlock Chap: ${chapterId}, Vol: ${volumeId}, Series: ${seriesId}`,
    });
}
