import React, { Component } from "react";
import { SessionDetails, Person } from "@soundbite/api";
import { ParticipantSummary } from "@soundbite/widgets-react";

export interface GroupRecorderProps {
  session?: SessionDetails;
  title?: string;
}

export class GroupRecorder extends Component<GroupRecorderProps> {
  getStatus(user: Person) {
    let clipForUser = this.props.session?.prompts[0].clips.filter((c) => {
      return c.contributor.route === user.route;
    });

    if (clipForUser) {
      return (
        <span>
          <span className="text-success">●</span>
          <small>Completed</small>
        </span>
      );
    } else {
      return (
        <span>
          <span className="text-danger">●</span>
          <small>Pending</small>
        </span>
      );
    }
  }

  render() {
    if (this.props.session === undefined) return <div />;

    return (
      <ParticipantSummary
        session={this.props.session}
        hideAudienceRoleWarning={true}
      />
    );
  }
}
