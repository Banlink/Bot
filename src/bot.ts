import {Client} from "@typeit/discord"
import {config} from "dotenv"
import {resolve} from "path"
import {Intents} from "discord.js";
import "reflect-metadata";


config({path: resolve(__dirname, "../.env")})


export class Main {
    private static _client: Client;

    static get Client(): Client {
        return this._client;
    }

    static start() {
        this._client = new Client({
            intents: [
                Intents.FLAGS.GUILD_INTEGRATIONS,
                Intents.FLAGS.GUILDS,
                Intents.FLAGS.GUILD_MESSAGES
            ],
            slashGuilds: ["796334983542341632"],
            requiredByDefault: true
        });

        this._client.login(process.env.BOT_TOKEN!,
            `${__dirname}/discords/*.ts`,
            `${__dirname}/discords/*.js`)

        this._client.once("ready", async () => {
            await this._client.clearSlashes();
            await this._client.initSlashes();

            console.log("Bot is now running :D");
        });

        this._client.on("interaction", (interaction) => {
            this._client.executeSlash(interaction);
        });
    }
}

Main.start();