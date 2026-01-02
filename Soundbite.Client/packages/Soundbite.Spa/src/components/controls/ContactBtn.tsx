import React from "react";
import { observer } from "mobx-react-lite";
import { Button } from "reactstrap";
import { WidgetStore } from "@soundbite/widgets-react";
import { Utils } from "@soundbite/api";

export interface IFeedbackBtnProps {
  subject?: string;
  text?: string;
  body?: string;
  to?: string;
  textOnly?: boolean;
  className?: string;
}

export const ContactBtn: React.FC<IFeedbackBtnProps> = observer(
  (props: IFeedbackBtnProps) => {
    // To
    const encodedTo = encodeURIComponent(
      props.to || "Support@Soundbite.Freshdesk.Com"
    );

    // Subject
    const text = props.text || "Support";
    const subject = props.subject || `${text}: [Your Subject]`;
    const encodedSubject = encodeURIComponent(subject);

    // Body
    const details = () => {
      const location = `Location: ${window.location.href}`;
      if (WidgetStore.users.currentUser) {
        const me = WidgetStore.users.currentUser;
        const signature = "Thank you,";
        const name = Utils.userDisplay(me);
        const email = me.email;
        return `\n\n${location}\n\n${signature}\n${name}\n${email}`;
      } else {
        return `\n\n${location}`;
      }
    };

    const body = props.body || "";
    const encodedBody = encodeURIComponent(`${body}\n\n${details()}\n\n`);

    const linkHref = `mailto:${encodedTo}?subject=${encodedSubject}&body=${encodedBody}`;

    // Wrap mode

    // Text Mode
    if (props.textOnly) {
      return (
        <a
          href={linkHref}
          className={props.className}
          rel="noopener noreferrer"
          title="E-Mail Soundbite Support"
        >
          {text}
        </a>
      );
    }

    // Button Mode
    const classes = `btn-icon btn-3 ${props.className} `;
    return (
      <a href={linkHref} target="_blank" rel="noopener noreferrer">
        <Button
          className={classes}
          type="button"
          title="E-Mail Soundbite Support"
        >
          <span className="btn-inner--icon mr-1">
            <i className="fas fa-comment"></i>
          </span>
          <span className="btn-inner--text">{text}</span>
        </Button>
      </a>
    );
  }
);
