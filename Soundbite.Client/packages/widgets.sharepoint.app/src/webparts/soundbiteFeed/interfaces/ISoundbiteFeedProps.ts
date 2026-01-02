import { WebPartContext } from "@microsoft/sp-webpart-base";
import SoundbiteFeedWebPart from "../SoundbiteFeedWebPart";

/**
 * Soundbite Feed component properties interface.
 */
export interface ISoundbiteFeedProps {
  part: SoundbiteFeedWebPart;
  context: WebPartContext;
  hideTitle?: boolean;
  groupRoute: string;
  title?: string;
}
