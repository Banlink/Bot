import {Client} from "@typeit/discord"
import {config} from "dotenv"
import {resolve} from "path"

config({path: resolve(__dirname, "../.env")})


export class Main {
    private static _client: Client;

    static get Client(): Client {
        return this._client;
    }

    static start() {
        this._client = new Client();
        this._client.login(process.env.BOT_TOKEN!,
            `${__dirname}/discords/*.ts`,
            `           ${__dirname}/discords/*.js`)
    }
}

Main.start();