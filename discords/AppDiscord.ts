import {Slash, Discord, Option, Group, Guild, Choices} from "@typeit/discord";
import {CommandInteraction} from "discord.js";

@Discord()
@Guild("796334983542341632")
@Group(
    "testing",
    "TESTING LOL",
    {
        text: "swag"
    }
)

export abstract class AppDiscord {
    @Slash("Ping")
    @Group("text")
    Ping(
        @Option("text")
        text: string,
        interaction: CommandInteraction
    ) {
        interaction.reply("SWAG");
    }
}