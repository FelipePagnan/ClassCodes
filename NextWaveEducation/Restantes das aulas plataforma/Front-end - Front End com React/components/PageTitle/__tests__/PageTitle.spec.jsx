import React from "react";
import { fireEvent, cleanup } from "@testing-library/react";
import { renderWithTheme } from "unit-tests/utils/renderWithTheme";
import PageTitle from "../PageTitle";

const actionButton = {
  label: "Teste",
  action: jest.fn(),
};

function mountComponent() {
  return renderWithTheme(
    <PageTitle title="Titulo da Página" actionButton={actionButton} />
  );
}

beforeEach(cleanup);

describe("[components] PageTitle", () => {
  it("should render PageTitle", () => {
    expect(mountComponent().container).toBeTruthy();
    expect(mountComponent().container).toHaveTextContent("Titulo da Página");
  });

  it("should onClick in PageTitle component", () => {
    const button = mountComponent().getByTestId("page-title-button");

    fireEvent.click(button)

    expect(actionButton.action).toHaveBeenCalled()
  });
});
